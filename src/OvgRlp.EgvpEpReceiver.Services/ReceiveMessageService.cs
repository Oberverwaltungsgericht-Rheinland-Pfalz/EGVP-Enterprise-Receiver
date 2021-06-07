using OvgRlp.Core.Types;
using OvgRlp.EgvpEpReceiver.Infrastructure.EgvpEnterpriseSoap;
using OvgRlp.EgvpEpReceiver.Infrastructure.Models;
using OvgRlp.Libs.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace OvgRlp.EgvpEpReceiver.Services
{
  public class ReceiveMessageService
  {
    private static EgvpPortTypeClient EgvpClient = new EgvpPortTypeClient();
    protected EgvpPostbox _egvpPostbox { get; set; }
    protected string _waitingHoursAfterError;
    protected string _tempDir;
    protected string _lockFile;

    public ReceiveMessageService(EgvpPostbox postbox, string waitingHoursAfterError, string tempDir, string lockFile)
    {
      _egvpPostbox = postbox;
      _waitingHoursAfterError = waitingHoursAfterError;
      _tempDir = tempDir;
      _lockFile = lockFile;
    }

    public void ReceiveMessages()
    {
      var requ = new getUncommittedMessageIDsRequest();
      var resp = new getUncommittedMessageIDsResponse();

      if (null == this._egvpPostbox)
        throw new ArgumentNullException("EgvpPostbox");

      requ.userID = this._egvpPostbox.Id;
      resp = EgvpClient.getUncommittedMessageIDs(requ);
      if (resp.returnCode != GetUncommittedMessageIDsReturnCodeType.OK)
        throw new Exception(string.Format("Fehler bei getUncommittedMessageIDs im Postfach {0}: {1}", this._egvpPostbox.Name, resp.returnCode.ToString()));

      if (null != resp.uncommittedMessages)
      {
        try
        {
          CreateLockFile();
          foreach (var msg in resp.uncommittedMessages)
          {
            if (ProtocolService.CheckDBMessageErrorFlag(msg.messageID, _waitingHoursAfterError))
              continue;
            ReceiveMessage(msg.messageID);
          }
          DeleteLockFile();
        }
        catch (Exception ex)
        {
          DeleteLockFile();
          throw ex;
        }
      }
    }

    public void ReceiveMessage(string messageId, bool createLockFile = false)
    {
      string logKontext = messageId;
      LogEntry logEntry = new LogEntry(String.Format("MessageId: {0}", messageId), LogEventLevel.Information);
      var logMetadata = new LogMetadata();
      var requ = new receiveMessageRequest();
      var resp = new receiveMessageResponse();
      var protService = new ProtocolService(EgvpClient);

      requ.messageID = messageId;
      requ.userID = this._egvpPostbox.Id;
      protService.CreateLogMetadata("", ref logMetadata, messageId, this._egvpPostbox);

      try
      {
        if (createLockFile)
          CreateLockFile();
        resp = EgvpClient.receiveMessage(requ);
        if (resp.returnCode != ReceiveReturnCodeType.OK)
          throw new Exception(string.Format("Fehler bei receiveMessage - ID {0}: {1}", messageId, resp.returnCode.ToString()));

        if (null == resp || null == resp.messageZIP)
          throw new Exception("Fehler bei receiveMessage - resp.messageZIP ist null");

        try
        {
          logEntry.AddSubEntry("Aufbau Metadaten für das Logging", LogEventLevel.Information);
          protService.CreateLogMetadata(resp, ref logMetadata, messageId, this._egvpPostbox);
        }
        catch (Exception ex)
        {
          logEntry.AddSubEntry(String.Format("Fehler bei 'Aufbau Metadaten für das Logging' aufgetreten ({0})", ex.Message), LogEventLevel.Warning);
          LoggingHelper.AddInnerExceptionToLogEntry(logEntry, ex, LogEventLevel.Warning);
        }

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

        if (createLockFile)
          DeleteLockFile();
      }
      catch (Exception ex)
      {
        if (createLockFile)
          DeleteLockFile();
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
      requ.userID = this._egvpPostbox.Id;
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

    // nur manuelle Ansteuerung möglich  (vorerst nicht aus der CLI)
    public static void ReceiveMessagesWithoutCommit(string PostboxId, string exportPath, string tempdir, bool write = false)
    {
      bool readOnly = !write;

      try
      {
        var requ = new getUncommittedMessageIDsRequest();
        var resp = new getUncommittedMessageIDsResponse();

        Console.WriteLine("Folgende Nachrichten werden gespeichert:");

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

              string zipTempFilename = Path.Combine(tempdir, msg.messageID + ".zip");
              string fullfilename = Path.Combine(exportPath, RecResp.messageID);
              if (!Directory.Exists(fullfilename))
              {
                File.WriteAllBytes(zipTempFilename, RecResp.messageZIP);
                ZipFile.ExtractToDirectory(zipTempFilename, fullfilename);
                File.Delete(zipTempFilename);
              }
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
      string zipFullFilename = Path.Combine(_tempDir, resp.messageID + ".zip");
      string fullfilename = "";

      try
      {
        logEntry.AddSubEntry(String.Format("Nachricht temporär zwischenspeichern nach {0}", zipFullFilename), LogEventLevel.Information);
        File.WriteAllBytes(zipFullFilename, resp.messageZIP);

        var exportPath = this._egvpPostbox.ExportPath;
        if (this._egvpPostbox.ExportPath_EEB.Count > 0 && IsEeb(resp.messageZIP))
          exportPath = this._egvpPostbox.ExportPath_EEB;

        foreach (string expPath in exportPath)
        {
          fullfilename = Path.Combine(expPath, resp.messageID);
          CheckAndFixAttachmentFails(zipFullFilename, fullfilename, logEntry);
          logEntry.AddSubEntry(String.Format("Nachricht exportieren nach {0}", fullfilename), LogEventLevel.Information);
          ZipFile.ExtractToDirectory(zipFullFilename, fullfilename);
        }

        try
        {
          foreach (string archPath in this._egvpPostbox.ArchivPath)
          {
            fullfilename = Path.Combine(DatetimeHelper.ReplaceDatetimeTags(archPath, DateTime.Now), resp.messageID);
            CheckAndFixAttachmentFails(zipFullFilename, fullfilename, logEntry);
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

    private void CheckAndFixAttachmentFails(string zipFullFilename, string targetFolder, LogEntry logEntry)
    {
      bool fixNames = false;

      try
      {
        using (ZipArchive za = ZipFile.OpenRead(zipFullFilename))
        {
          var entries = za.Entries.ToArray();
          foreach (var entry in entries)
          {
            if (entry.Name != string.Empty)
            {
              if (Path.Combine(targetFolder, entry.FullName).Length >= 256)
              {
                fixNames = true;
              }
              if (entries.Where(e => e.FullName.Trim().ToLower() == entry.FullName.Trim().ToLower()).ToArray().Length > 1)
              {
                fixNames = true;
              }
            }
          }
        }

        if (fixNames)
        {
          using (var za = new ZipArchive(File.Open(zipFullFilename, FileMode.Open, FileAccess.ReadWrite), ZipArchiveMode.Update))
          {
            var entries = za.Entries.ToArray();
            var nameList = new List<string>();
            foreach (var entry in entries)
            {
              string newName = string.Empty;
              string subdir = Path.GetDirectoryName(entry.FullName);
              string oldName = entry.Name;

              if (oldName != string.Empty)
              {
                if (nameList.Exists(n => n.Trim().ToLower() == entry.FullName.Trim().ToLower()))
                {
                  var rand = new Random();
                  newName = Path.GetFileNameWithoutExtension(oldName) + "_" + rand.Next(10, 99).ToString() + Path.GetExtension(oldName);
                  logEntry.AddSubEntry(String.Format("Der Dateiname der Anlage '{0}' existiert wurde vom Absender doppelt vergeben, es erfolgt eine Umbenennung zu {1}", oldName, newName), LogEventLevel.Warning);
                }
                if (Path.Combine(targetFolder, entry.FullName).Length >= 256)
                {
                  newName = Guid.NewGuid().ToString() + Path.GetExtension(oldName);
                  logEntry.AddSubEntry(String.Format("Der Dateiname der Anlage '{0}' ist zu lang, es erfolgt eine Umbenennung zu {1}", oldName, newName), LogEventLevel.Warning);
                }

                if (!string.IsNullOrEmpty(newName))
                {
                  var newEntry = za.CreateEntry(Path.Combine(subdir, newName));
                  using (var a = entry.Open())
                  using (var b = newEntry.Open())
                    a.CopyTo(b);
                  entry.Delete();
                }
                else
                {
                  nameList.Add(entry.FullName);
                }
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        logEntry.AddSubEntry(String.Format("Fehler bei CheckAndFixAttachmentNamelength: {0}", ex.Message), LogEventLevel.Warning);
      }
    }

    private void CommitMessage(string messageId, LogEntry logEntry)
    {
      var requ = new commitReceivedMessageRequest();
      var resp = new commitReceivedMessageResponse();

      requ.messageID = messageId;
      requ.userID = this._egvpPostbox.Id;

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

    private void CreateLockFile()
    {
      foreach (string exportPath in this._egvpPostbox.ExportPath)
      {
        if (!string.IsNullOrEmpty(_lockFile))
        {
          string lockfile = Path.Combine(DatetimeHelper.ReplaceDatetimeTags(exportPath, DateTime.Now), _lockFile);
          if (!File.Exists(lockfile))
            File.Create(lockfile).Dispose();
        }
      }
    }

    private void DeleteLockFile()
    {
      foreach (string exportPath in this._egvpPostbox.ExportPath)
      {
        if (!string.IsNullOrEmpty(_lockFile))
        {
          string lockfile = Path.Combine(DatetimeHelper.ReplaceDatetimeTags(exportPath, DateTime.Now), _lockFile);
          if (File.Exists(lockfile))
            File.Delete(lockfile);
        }
      }
    }
  }
}