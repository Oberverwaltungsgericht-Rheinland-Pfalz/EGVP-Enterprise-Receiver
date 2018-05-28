using OvgRlp.EgvpEpFetcher.Infrastructure.Models;
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
    }
}