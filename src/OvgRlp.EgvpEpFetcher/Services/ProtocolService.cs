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
        // Logging Metadaten aufbereiten
        public static void CreateLogMetadata(EGVPMessageProps msgProps, ref LogMetadata logMetadata, string messageID = null, EgvpPostbox EgvpPostbox = null, string MessageSizeKB = null)
        {
            if (null == logMetadata)
                logMetadata = new LogMetadata();
            logMetadata.AppVersion = CommonHelper.AssemblyVersion();

            if (null != messageID)
                logMetadata.MessageId = messageID;
            if (null != MessageSizeKB)
                logMetadata.MessageSizeKB = MessageSizeKB;
            if (null != EgvpPostbox)
            {
                logMetadata.Recipient = EgvpPostbox.Id;
                logMetadata.RecipientName = EgvpPostbox.Name;
            }

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
        public static void CreateLogMetadata(EgvpEnterpriseSoap.receiveMessageResponse resp, ref LogMetadata logMetadata, string messageID = null, EgvpPostbox EgvpPostbox = null)
        {
            EGVPMessageProps msgProps = null;
            string MessageSizeKB = "";

            if (null != resp)
            {
                try { MessageSizeKB = Convert.ToString((Convert.ToInt32(resp.messageZIP.Length) / 1024)); }
                catch { MessageSizeKB = ""; }

                using (ZipArchive za = new ZipArchive(new MemoryStream(resp.messageZIP)))
                {
                    var ze = za.GetEntry("MsgProps.xml");
                    using (var stream = ze.Open())
                    {
                        msgProps = EGVPMessageProps.LoadFromStream(stream);
                    }
                }
            }

            CreateLogMetadata(msgProps, ref logMetadata, messageID, EgvpPostbox, MessageSizeKB);
        }

        // Logging Metadaten aufbereiten
        public static void CreateLogMetadata(string zipFullFilename, ref LogMetadata logMetadata, string messageID = null, EgvpPostbox EgvpPostbox = null)
        {
            EGVPMessageProps msgProps = null;
            string MessageSizeKB = "";

            if (!string.IsNullOrEmpty(zipFullFilename))
            {
                try { MessageSizeKB = Convert.ToString((Convert.ToInt32(new FileInfo(zipFullFilename).Length) / 1024)); }
                catch { MessageSizeKB = ""; }

                using (ZipArchive za = ZipFile.OpenRead(zipFullFilename))
                {
                    var ze = za.GetEntry("MsgProps.xml");
                    using (var stream = ze.Open())
                    {
                        msgProps = EGVPMessageProps.LoadFromStream(stream);
                    }
                }
            }

            CreateLogMetadata(msgProps, ref logMetadata, messageID, EgvpPostbox, MessageSizeKB);
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
    }
}