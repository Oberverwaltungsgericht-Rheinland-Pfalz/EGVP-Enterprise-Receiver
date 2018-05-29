using OvgRlp.EgvpEpFetcher.EgvpEnterpriseSoap;
using OvgRlp.EgvpEpFetcher.Infrastructure;
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
    public class ProtocolService
    {
        private EgvpPortTypeClient EgvpClient = null;

        public ProtocolService(EgvpPortTypeClient egvpClient)
        {
            this.EgvpClient = egvpClient;
        }

        // Logging Metadaten aufbereiten
        public void CreateLogMetadata(EGVPMessageProps msgProps, ref LogMetadata logMetadata, string messageID = null, EgvpPostbox egvpPostbox = null, string messageSizeKB = null)
        {
            if (null == logMetadata)
                logMetadata = new LogMetadata();
            logMetadata.AppVersion = CommonHelper.AssemblyVersion();

            if (null != messageID)
                logMetadata.MessageId = messageID;
            if (null != messageSizeKB)
                logMetadata.MessageSizeKB = messageSizeKB;
            if (null != egvpPostbox)
            {
                logMetadata.Recipient = egvpPostbox.Id;
                logMetadata.RecipientName = egvpPostbox.Name;
            }

            ExtendedLogMetadataFromState(messageID, egvpPostbox, ref logMetadata);

            if (null != msgProps)
            {
                if (!String.IsNullOrEmpty(msgProps.MessageID))
                    logMetadata.MessageId = msgProps.MessageID;
                if (!String.IsNullOrEmpty(msgProps.MsgSubject))
                    logMetadata.Subject = msgProps.MsgSubject;
                if (!String.IsNullOrEmpty(msgProps.OSCISubject))
                    logMetadata.MessageType = msgProps.OSCISubject;
                if (null != msgProps.AddresseeCertProps)
                {
                    if (!String.IsNullOrEmpty(msgProps.AddresseeCertProps.Name))
                        logMetadata.RecipientName = msgProps.AddresseeCertProps.Name;
                }
                if (null != msgProps.Originator && null != msgProps.Originator.BusinessCard)
                {
                    if (!String.IsNullOrEmpty(msgProps.Originator.BusinessCard.UserID))
                        logMetadata.Sender = msgProps.Originator.BusinessCard.UserID;
                    if (!String.IsNullOrEmpty(msgProps.Originator.BusinessCard.Name))
                        logMetadata.SenderName = msgProps.Originator.BusinessCard.Name;
                }
            }
        }

        // Logging Metadaten aufbereiten
        public void CreateLogMetadata(EgvpEnterpriseSoap.receiveMessageResponse resp, ref LogMetadata logMetadata, string messageID = null, EgvpPostbox egvpPostbox = null)
        {
            EGVPMessageProps msgProps = null;
            string messageSizeKB = "";

            if (null != resp)
            {
                try { messageSizeKB = Convert.ToString((Convert.ToInt32(resp.messageZIP.Length) / 1024)); }
                catch { messageSizeKB = ""; }

                using (ZipArchive za = new ZipArchive(new MemoryStream(resp.messageZIP)))
                {
                    var ze = za.GetEntry("MsgProps.xml");
                    using (var stream = ze.Open())
                    {
                        msgProps = EGVPMessageProps.LoadFromStream(stream);
                    }
                }
            }

            CreateLogMetadata(msgProps, ref logMetadata, messageID, egvpPostbox, messageSizeKB);
        }

        // Logging Metadaten aufbereiten
        public void CreateLogMetadata(string zipFullFilename, ref LogMetadata logMetadata, string messageID = null, EgvpPostbox egvpPostbox = null)
        {
            EGVPMessageProps msgProps = null;
            string messageSizeKB = "";

            if (!string.IsNullOrEmpty(zipFullFilename))
            {
                try { messageSizeKB = Convert.ToString((Convert.ToInt32(new FileInfo(zipFullFilename).Length) / 1024)); }
                catch { messageSizeKB = ""; }

                using (ZipArchive za = ZipFile.OpenRead(zipFullFilename))
                {
                    var ze = za.GetEntry("MsgProps.xml");
                    using (var stream = ze.Open())
                    {
                        msgProps = EGVPMessageProps.LoadFromStream(stream);
                    }
                }
            }

            CreateLogMetadata(msgProps, ref logMetadata, messageID, egvpPostbox, messageSizeKB);
        }

        // Hinweis auf einen Fehlerhaften Nachrichtenabruf in die Datenbank aufnehmen
        public static void CreateDBMessageErrorFlag(string messageID, string description)
        {
            try
            {
                using (var db = new APPDATAEntities())
                {
                    var qry = from EgvpEnterpriseFetcher_Error in db.EgvpEnterpriseFetcher_Error
                              where EgvpEnterpriseFetcher_Error.MessageId == messageID
                              select EgvpEnterpriseFetcher_Error;
                    EgvpEnterpriseFetcher_Error errData;

                    if (qry.Count() > 0)
                    { errData = qry.First(); }
                    else
                    { errData = new EgvpEnterpriseFetcher_Error() { MessageId = messageID }; }

                    errData.Description = description;
                    errData.TimeStamp = DateTime.Now;

                    if (qry.Count() < 1) { db.EgvpEnterpriseFetcher_Error.Add(errData); }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, "CreateDBMessageErrorFlag: Fehler beim Erzeugen eines Fehlerhinweises zu Nachricht " + messageID, LogEventLevel.Warning);
            }
        }

        // Prüfen ob ein Fehlerhinweis zu einer Nachricht existiert und diese evtl. nicht abgerufen werden soll
        public static bool CheckDBMessageErrorFlag(string messageID)
        {
            bool rval = false;

            try
            {
                using (var db = new APPDATAEntities())
                {
                    var qry = from EgvpEnterpriseFetcher_Error in db.EgvpEnterpriseFetcher_Error
                              where EgvpEnterpriseFetcher_Error.MessageId == messageID
                              select EgvpEnterpriseFetcher_Error;

                    if (qry.Count() > 0)
                    {
                        EgvpEnterpriseFetcher_Error errData = qry.First();

                        Int32 waitingHours = 0;
                        if (!string.IsNullOrEmpty(Properties.Settings.Default.WaitingHoursAfterError))
                            waitingHours = Convert.ToInt32(Properties.Settings.Default.WaitingHoursAfterError);

                        if (null != errData.TimeStamp && waitingHours > 0)
                        {
                            DateTime cmp = errData.TimeStamp;
                            if (DateTime.Compare(DateTime.Now, cmp.AddHours(waitingHours)) < 0)
                                rval = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, "CheckDBMessageErrorFlag: Fehler beim Prüfen eines Fehlerhinweises zu Nachricht " + messageID, LogEventLevel.Warning);
            }
            return rval;
        }

        // weitere Metadaten per Soap abrufen
        private void ExtendedLogMetadataFromState(string messageID, EgvpPostbox egvpPostbox, ref LogMetadata logMetadata)
        {
            try
            {
                if (null != EgvpClient)
                {
                    var requ = new getStateRequest();
                    var resp = new getStateResponse();
                    requ.customOrMessageID = messageID;
                    requ.userID = egvpPostbox.Id;
                    resp = this.EgvpClient.getState(requ);
                    if (resp.returnCode != GetStateReturnCodeType.OK)
                        throw new Exception(resp.returnCode.ToString());

                    logMetadata.Recipient = resp.receiverID;
                    logMetadata.Sender = resp.senderID;
                    logMetadata.OsciState = resp.state.ToString();
                    logMetadata.OsciDatetime = resp.time.ToShortDateString() + " " + resp.time.ToLongTimeString();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, "ExtendedLogMetadataFromState: Fehler beim Abrufen des Nachrichtenstatus - MessageId: " + messageID, LogEventLevel.Warning);
            }
        }
    }
}