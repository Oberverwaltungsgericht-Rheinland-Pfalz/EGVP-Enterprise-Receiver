using OvgRlp.Core.Types;
using OvgRlp.EgvpEpReceiver.EgvpEnterpriseSoap;
using OvgRlp.EgvpEpReceiver.Infrastructure.Models;
using OvgRlp.Libs.Logging;
using System;
using System.IO;
using System.IO.Compression;

namespace OvgRlp.EgvpEpReceiver.Services
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

    public getStateResponse GetMessageState(string messageID)
    {
      var requ = new getStateRequest();
      var resp = new getStateResponse();
      requ.customOrMessageID = messageID;
      requ.userID = this.EgvpPostbox.Id;
      resp = EgvpClient.getState(requ);
      if (resp.returnCode != GetStateReturnCodeType.OK)
        throw new Exception(resp.returnCode.ToString());
      return resp;
    }

    // nur manuelle Ansteuerung möglich  (vorerst nicht aus der CLI)
    public static void CommmitAllUncommittedMessages(string PostboxId, bool write = false)
    {
      bool readOnly = !write;
      try
      {
        var requ = new getUncommittedMessageIDsRequest();
        var resp = new getUncommittedMessageIDsResponse();

        if (!readOnly)
          Console.WriteLine("Nachrichten werden als abgeholt gekennzeichnet:");
        else
          Console.WriteLine("Folgende Nachrichten sind als nicht abgeholt gekennzeichnet:");

        requ.userID = PostboxId;
        resp = EgvpClient.getUncommittedMessageIDs(requ);
        if (resp.returnCode != GetUncommittedMessageIDsReturnCodeType.OK)
          throw new Exception(string.Format("Fehler bei getUncommittedMessageIDs im Postfach {0}: {1}", PostboxId, resp.returnCode.ToString()));

        if (null != resp.uncommittedMessages)
        {
          foreach (var msg in resp.uncommittedMessages)
          {
            Console.WriteLine(msg.messageID + " " + msg.osciDate);

            if (!readOnly)
            {
              var RecRequ = new receiveMessageRequest();
              var RecResp = new receiveMessageResponse();
              RecRequ.messageID = msg.messageID;
              RecRequ.userID = PostboxId;
              RecResp = EgvpClient.receiveMessage(RecRequ);

              var comRequ = new commitReceivedMessageRequest();
              var comResp = new commitReceivedMessageResponse();
              comRequ.messageID = msg.messageID;
              comRequ.userID = PostboxId;
              comResp = EgvpClient.commitReceivedMessage(comRequ);
              if (comResp.returnCode != CommitReturnCodeType.OK)
                Console.WriteLine("Fehler bei commitReceivedMessage: {0}", comResp.returnCode.ToString());
            }
          }

          Console.WriteLine("");
          Console.WriteLine("Insgesamt {0} Nachrichten", resp.uncommittedMessages.Length);
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine("Unbekannter Fehler: {0}", ex.Message.ToString());
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

        var exportPath = this.EgvpPostbox.ExportPath;
        if (this.EgvpPostbox.ExportPath_EEB.Count > 0 && IsEeb(resp.messageZIP))
          exportPath = this.EgvpPostbox.ExportPath_EEB;

        foreach (string expPath in exportPath)
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

        logEntry.AddSubEntry(String.Format("Temporär zwischengespeicherte Nachricht wieder löschen", zipFullFilename), LogEventLevel.Information);
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

    private bool IsEeb(byte[] messageZIP)
    {
      bool rval = false;
      using (ZipArchive za = new ZipArchive(new MemoryStream(messageZIP)))
      {
        ZipArchiveEntry ze = za.GetEntry("attachments/xjustiz_nachricht.xml");
        if (null != ze)
        {
          TextReader tr = new StreamReader(ze.Open());
          if (tr.ReadToEnd().Contains("nachricht.eeb.zuruecklaufend"))
            rval = true;
        }
      }
      return rval;
    }
  }
}