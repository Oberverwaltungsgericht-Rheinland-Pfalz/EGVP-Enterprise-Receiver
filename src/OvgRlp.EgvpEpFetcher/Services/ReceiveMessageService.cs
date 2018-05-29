using OvgRlp.EgvpEpFetcher.EgvpEnterpriseSoap;
using OvgRlp.EgvpEpFetcher.Infrastructure.Models;
using OvgRlp.Libs.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OvgRlp.EgvpEpFetcher.Services
{
    public class ReceiveMessageService
    {
        private static EgvpPortTypeClient EgvpClient = new EgvpEnterpriseSoap.EgvpPortTypeClient();
        public EgvpPostbox EgvpPostbox { get; set; }

        public ReceiveMessageService(EgvpPostbox postbox)
        {
            this.EgvpPostbox = postbox;
        }

        public void ReceiveMessages()
        {
            var requ = new getUncommittedMessageIDsRequest();
            var resp = new getUncommittedMessageIDsResponse();

            if (null == this.EgvpPostbox)
                throw new ArgumentNullException("EgvpPostbox");

            requ.userID = this.EgvpPostbox.Id;
            resp = EgvpClient.getUncommittedMessageIDs(requ);
            if (resp.returnCode != GetUncommittedMessageIDsReturnCodeType.OK)
                throw new Exception(string.Format("Fehler bei getUncommittedMessageIDs im Postfach {0}: {1}", this.EgvpPostbox.Name, resp.returnCode.ToString()));

            if (null != resp.uncommittedMessages)
            {
                foreach (var msg in resp.uncommittedMessages)
                {
                    if (ProtocolService.CheckDBMessageErrorFlag(msg.messageID))
                        continue;
                    ReceiveMessage(msg.messageID);
                }
            }
        }

        public void ReceiveMessage(string messageId)
        {
            string logKontext = messageId;
            LogEntry logEntry = new LogEntry(String.Format("MessageId: {0}", messageId), LogEventLevel.Information);
            var logMetadata = new LogMetadata();
            var requ = new receiveMessageRequest();
            var resp = new receiveMessageResponse();
            var protService = new ProtocolService(EgvpClient);

            requ.messageID = messageId;
            requ.userID = this.EgvpPostbox.Id;
            protService.CreateLogMetadata("", ref logMetadata, messageId, this.EgvpPostbox);

            try
            {
                resp = EgvpClient.receiveMessage(requ);
                if (resp.returnCode != ReceiveReturnCodeType.OK)
                    throw new Exception(string.Format("Fehler bei receiveMessage - ID {0}: {1}", messageId, resp.returnCode.ToString()));

                logEntry.AddSubEntry("Aufbau Metadaten für das Logging", LogEventLevel.Information);
                protService.CreateLogMetadata(resp, ref logMetadata, messageId, this.EgvpPostbox);

                try
                {
                    logEntry.AddSubEntry("Start Verarbeitung", LogEventLevel.Information);
                    ExtractFiles(resp, logEntry);
                }
                catch (Exception ex)
                { throw new Exception("Fehler bei ExtractFiles aufgetreten", ex); }

                try
                {
                    logEntry.AddSubEntry("Nachricht als empfangen markieren", LogEventLevel.Information);
                    CommitMessage(messageId, logEntry);
                }
                catch (Exception ex)
                { throw new Exception("Fehler bei CommitMessage aufgetreten", ex); }

                logEntry.AddSubEntry("Nachricht wurde erfolgreich verarbeitet!", LogEventLevel.Information);
                Logger.Log(logEntry, logKontext, logMetadata);
            }
            catch (Exception ex)
            {
                logEntry.AddSubEntry(String.Format("Abbruch bei Verarbeitung der Nachricht ({0})", ex.Message), LogEventLevel.Error);
                LoggingHelper.AddInnerExceptionToLogEntry(logEntry, ex);
                Logger.Log(logEntry, logKontext, logMetadata);
                ProtocolService.CreateDBMessageErrorFlag(messageId, ex.Message);
            }
        }

        private void ExtractFiles(receiveMessageResponse resp, LogEntry logEntry)
        {
            string zipFullFilename = Path.Combine(Properties.Settings.Default.tempDir, resp.messageID + ".zip");
            string fullfilename = "";

            try
            {
                logEntry.AddSubEntry(String.Format("Nachricht temporär zwischenspeichern nach {0}", zipFullFilename), LogEventLevel.Information);
                File.WriteAllBytes(zipFullFilename, resp.messageZIP);

                foreach (string expPath in this.EgvpPostbox.ExportPath)
                {
                    fullfilename = Path.Combine(expPath, resp.messageID);
                    logEntry.AddSubEntry(String.Format("Nachricht exportieren nach {0}", fullfilename), LogEventLevel.Information);
                    ZipFile.ExtractToDirectory(zipFullFilename, fullfilename);
                }

                try
                {
                    foreach (string archPath in this.EgvpPostbox.ArchivPath)
                    {
                        fullfilename = Path.Combine(DatetimeHelper.ReplaceDatetimeTags(archPath, DateTime.Now), resp.messageID);
                        logEntry.AddSubEntry(String.Format("Nachricht Archivieren nach {0}", fullfilename), LogEventLevel.Information);
                        ZipFile.ExtractToDirectory(zipFullFilename, fullfilename);
                    }
                }
                catch (Exception ex)
                {
                    string ft = String.Format("Nachricht konnte nicht in das Archivverzeichnis {0} kopiert werden: {1}", fullfilename, ex.Message);
                    logEntry.AddSubEntry(ft, LogEventLevel.Warning);
                }

                logEntry.AddSubEntry(String.Format("Temporär zwischengespeicherte Nachrich wieder löschen", zipFullFilename), LogEventLevel.Information);
                File.Delete(zipFullFilename);
            }
            catch (Exception ex)
            {
                try { File.Delete(zipFullFilename); }
                catch { /*ohne Fehlerbehandlung*/}
                throw ex;
            }
        }

        private void CommitMessage(string messageId, LogEntry logEntry)
        {
            var requ = new commitReceivedMessageRequest();
            var resp = new commitReceivedMessageResponse();

            requ.messageID = messageId;
            requ.userID = this.EgvpPostbox.Id;

            resp = EgvpClient.commitReceivedMessage(requ);
            if (resp.returnCode != CommitReturnCodeType.OK)
            {
                string ft = string.Format("Fehler bei commitReceivedMessage - ID {0}: {1}", messageId, resp.returnCode.ToString());
                if (resp.returnCode != CommitReturnCodeType.MESSAGE_ALREADY_COMMITTED)
                { throw new Exception(ft); }
                else
                { logEntry.AddSubEntry(ft, LogEventLevel.Warning); }
            }
        }
    }
}