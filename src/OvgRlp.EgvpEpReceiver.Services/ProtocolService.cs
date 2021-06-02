using OvgRlp.EgvpEpReceiver.Infrastructure;
using OvgRlp.EgvpEpReceiver.Infrastructure.EgvpEnterpriseSoap;
using OvgRlp.EgvpEpReceiver.Infrastructure.Models;
using OvgRlp.Libs.Logging;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace OvgRlp.EgvpEpReceiver.Services
{
  public class ProtocolService
  {
    protected EgvpPortTypeClient _egvpClient = null;

    public ProtocolService(EgvpPortTypeClient egvpClient)
    {
      _egvpClient = egvpClient;
    }

    // Logging Metadaten aufbereiten
    public void CreateLogMetadata(EGVPMessageProps msgProps, ref LogMetadata logMetadata, string messageID = null, EgvpPostbox egvpPostbox = null, string messageSizeKB = null, string messageSizeAttachmentsKB = null)
    {
      if (null == logMetadata)
        logMetadata = new LogMetadata();
      logMetadata.AppVersion = OvgRlp.Core.Common.AssemblyHelper.AssemblyVersion(System.Reflection.Assembly.GetExecutingAssembly());

      if (null != messageID)
        logMetadata.MessageId = messageID;
      if (null != messageSizeKB)
        logMetadata.MessageSizeKB = messageSizeKB;
      if (null != messageSizeAttachmentsKB)
        logMetadata.MessageSizeAttachmentsKB = messageSizeAttachmentsKB;
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
    public void CreateLogMetadata(receiveMessageResponse resp, ref LogMetadata logMetadata, string messageID = null, EgvpPostbox egvpPostbox = null)
    {
      EGVPMessageProps msgProps = null;
      string messageSizeKB = "";
      string messageSizeAttachmentsKB = "";

      if (null != resp)
      {
        try { messageSizeKB = Convert.ToString((Convert.ToInt32(resp.messageZIP.Length) / 1024)); }
        catch { messageSizeKB = ""; }
        try { messageSizeAttachmentsKB = Convert.ToString((GetAttachmentsSize(resp.messageZIP) / 1024)); }
        catch { messageSizeAttachmentsKB = ""; }

        try
        {
          using (ZipArchive za = new ZipArchive(new MemoryStream(resp.messageZIP)))
          {
            var ze = za.GetEntry("MsgProps.xml");
            if (null != ze)
            {
              using (var stream = ze.Open())
              {
                msgProps = EGVPMessageProps.LoadFromStream(stream);
              }
            }
          }
        }
        catch (Exception ex)
        {
          CreateLogMetadata(null, ref logMetadata, messageID, egvpPostbox, messageSizeKB, messageSizeAttachmentsKB);
          throw ex;
        }
      }

      CreateLogMetadata(msgProps, ref logMetadata, messageID, egvpPostbox, messageSizeKB, messageSizeAttachmentsKB);
    }

    // Logging Metadaten aufbereiten
    public void CreateLogMetadata(string zipFullFilename, ref LogMetadata logMetadata, string messageID = null, EgvpPostbox egvpPostbox = null)
    {
      EGVPMessageProps msgProps = null;
      string messageSizeKB = "";
      string messageSizeAttachmentsKB = "";

      if (!string.IsNullOrEmpty(zipFullFilename))
      {
        try { messageSizeKB = Convert.ToString((Convert.ToInt32(new FileInfo(zipFullFilename).Length) / 1024)); }
        catch { messageSizeKB = ""; }
        try { messageSizeAttachmentsKB = Convert.ToString((GetAttachmentsSize(File.ReadAllBytes(zipFullFilename)) / 1024)); }
        catch { messageSizeAttachmentsKB = ""; }

        using (ZipArchive za = ZipFile.OpenRead(zipFullFilename))
        {
          var ze = za.GetEntry("MsgProps.xml");
          using (var stream = ze.Open())
          {
            msgProps = EGVPMessageProps.LoadFromStream(stream);
          }
        }
      }

      CreateLogMetadata(msgProps, ref logMetadata, messageID, egvpPostbox, messageSizeKB, messageSizeAttachmentsKB);
    }

    // Hinweis auf einen Fehlerhaften Nachrichtenabruf in die Datenbank aufnehmen
    public static void CreateDBMessageErrorFlag(string messageID, string description)
    {
      try
      {
        using (var db = new APPDATAEntities())
        {
          var qry = from EgvpEnterpriseReceiver_Error in db.EgvpEnterpriseReceiver_Error
                    where EgvpEnterpriseReceiver_Error.MessageId == messageID
                    select EgvpEnterpriseReceiver_Error;
          EgvpEnterpriseReceiver_Error errData;

          if (qry.Count() > 0)
          { errData = qry.First(); }
          else
          { errData = new EgvpEnterpriseReceiver_Error() { MessageId = messageID }; }

          errData.Description = description;
          errData.TimeStamp = DateTime.Now;

          if (qry.Count() < 1) { db.EgvpEnterpriseReceiver_Error.Add(errData); }
          db.SaveChanges();
        }
      }
      catch (Exception ex)
      {
        Logger.Log(ex.Message, "CreateDBMessageErrorFlag: Fehler beim Erzeugen eines Fehlerhinweises zu Nachricht " + messageID, LogEventLevel.Warning);
      }
    }

    // Prüfen ob ein Fehlerhinweis zu einer Nachricht existiert und diese evtl. nicht abgerufen werden soll
    public static bool CheckDBMessageErrorFlag(string messageID, string waitingHours)
    {
      bool rval = false;

      try
      {
        using (var db = new APPDATAEntities())
        {
          var qry = from EgvpEnterpriseReceiver_Error in db.EgvpEnterpriseReceiver_Error
                    where EgvpEnterpriseReceiver_Error.MessageId == messageID
                    select EgvpEnterpriseReceiver_Error;

          if (qry.Count() > 0)
          {
            EgvpEnterpriseReceiver_Error errData = qry.First();

            Int32 waitHours = 0;
            if (!string.IsNullOrEmpty(waitingHours))
              waitHours = Convert.ToInt32(waitingHours);

            if (null != errData.TimeStamp && waitHours > 0)
            {
              DateTime cmp = errData.TimeStamp;
              if (DateTime.Compare(DateTime.Now, cmp.AddHours(waitHours)) < 0)
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

    // MessageSizeAttachments ermitteln
    private int GetAttachmentsSize(byte[] messageZIP)
    {
      int size = 0;
      using (ZipArchive za = new ZipArchive(new MemoryStream(messageZIP), ZipArchiveMode.Read))
      {
        foreach (ZipArchiveEntry ze in za.Entries)
        {
          if (ze.FullName.StartsWith("attachments/"))
            size += Convert.ToInt32(ze.Length);
        }
      }
      return size;
    }

    // weitere Metadaten per Soap abrufen
    private void ExtendedLogMetadataFromState(string messageID, EgvpPostbox egvpPostbox, ref LogMetadata logMetadata)
    {
      try
      {
        var receiveMessageService = new ReceiveMessageService(egvpPostbox, "", "", "");  //TODO: refactor
        getStateResponse resp = receiveMessageService.GetMessageState(messageID);

        logMetadata.Recipient = resp.receiverID;
        logMetadata.Sender = resp.senderID;
        logMetadata.OsciState = resp.state.ToString();
        logMetadata.OsciDatetime = resp.time.ToShortDateString() + " " + resp.time.ToLongTimeString();
      }
      catch (Exception ex)
      {
        Logger.Log(ex.Message, "ExtendedLogMetadataFromState: Fehler beim Abrufen des Nachrichtenstatus - MessageId: " + messageID, LogEventLevel.Warning);
      }
    }
  }
}