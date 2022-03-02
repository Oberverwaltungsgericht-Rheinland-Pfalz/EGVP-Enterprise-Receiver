using OvgRlp.Core.Types;
using OvgRlp.EgvpEpReceiver.Infrastructure.Contracts;
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
  // TODO: weitere Refaktorisierungen, analog zu MessageSource sollte es auch ein MessageTarget geben
  public class ReceiveMessageService
  {
    protected EgvpPostbox _egvpPostbox { get; set; }
    protected IMessageSource _messageSource;
    protected string _waitingHoursAfterError;
    protected string _tempDir;
    protected string _lockFile;

    public ReceiveMessageService(IMessageSource messageSource, EgvpPostbox postbox, string waitingHoursAfterError, string tempDir, string lockFile)
    {
      _egvpPostbox = postbox;
      _waitingHoursAfterError = waitingHoursAfterError;
      _tempDir = tempDir;
      _lockFile = lockFile;
      _messageSource = messageSource;
    }

    public void ReceiveMessages()
    {
      List<MessageIdent> msgIds;

      if (null == this._egvpPostbox)
        throw new ArgumentNullException("EgvpPostbox");

      // Check new Messages
      msgIds = _messageSource.getNewMessages(this._egvpPostbox.Id, this._egvpPostbox.Name);

      if (null != msgIds && msgIds.Count > 0)
      {
        try
        {
          CreateLockFile();
          foreach (var msg in msgIds)
          {
            if (ProtocolService.CheckDBMessageErrorFlag(msg.MessageId, _waitingHoursAfterError))
              continue;
            ReceiveMessage(msg.MessageId);
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
      var protService = new ProtocolService(_messageSource);   // TODO: Refactor

      var msgIdent = new MessageIdent { MessageId = messageId, ReceiverId = this._egvpPostbox.Id };
      protService.CreateLogMetadata("", ref logMetadata, msgIdent.MessageId, this._egvpPostbox);

      try
      {
        // create LockFile (optional)
        if (createLockFile)
          CreateLockFile();

        // Receive Message
        Message message = _messageSource.ReceiveMessage(msgIdent);
        if (null == message || null == message.MessageData)
          throw new Exception("Fehler bei receiveMessage - MessageData ist null");

        // create Log
        try
        {
          logEntry.AddSubEntry("Aufbau Metadaten für das Logging", LogEventLevel.Information);
          protService.CreateLogMetadata(message, ref logMetadata, messageId, this._egvpPostbox);
        }
        catch (Exception ex)
        {
          logEntry.AddSubEntry(String.Format("Fehler bei 'Aufbau Metadaten für das Logging' aufgetreten ({0})", ex.Message), LogEventLevel.Warning);
          LoggingHelper.AddInnerExceptionToLogEntry(logEntry, ex, LogEventLevel.Warning);
        }

        // extract Files
        try
        {
          logEntry.AddSubEntry("Start Verarbeitung", LogEventLevel.Information);
          ExtractFiles(message, logEntry);
        }
        catch (Exception ex)
        { throw new Exception("Fehler bei ExtractFiles aufgetreten", ex); }

        // commit Message
        try
        {
          logEntry.AddSubEntry("Nachricht als empfangen markieren", LogEventLevel.Information);
          CommitMessage(message, logEntry);
        }
        catch (Exception ex)
        { throw new Exception("Fehler bei CommitMessage aufgetreten", ex); }

        // create Finished Log
        logEntry.AddSubEntry("Nachricht wurde erfolgreich verarbeitet!", LogEventLevel.Information);
        Logger.Log(logEntry, logKontext, logMetadata);

        // delete LockFile  (optional)
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

    // nur manuelle Ansteuerung möglich  (vorerst nicht aus der CLI)
    public static void CommmitAllUncommittedMessages(string PostboxId, bool write = false)
    {
      bool readOnly = !write;

      try
      {
        List<MessageIdent> msgIds;

        if (!readOnly)
          Console.WriteLine("Nachrichten werden als abgeholt gekennzeichnet:");
        else
          Console.WriteLine("Folgende Nachrichten sind als nicht abgeholt gekennzeichnet:");

        var messageSource = new MsgSrcEgvpEpWebservice();
        msgIds = messageSource.getNewMessages(PostboxId);

        if (null != msgIds && msgIds.Count > 0)
        {
          foreach (var msg in msgIds)
          {
            Console.WriteLine(msg.MessageId);

            if (!readOnly)
            {
              Message message = null;
              try
              { message = messageSource.ReceiveMessage(msg); }
              catch (Exception ex)
              { Console.WriteLine("Fehler: " + ex.Message); }

              string fh = "";
              try
              { fh = messageSource.CommitMessage(message); }
              catch (Exception ex)
              { fh = ex.Message; }
              if (!string.IsNullOrEmpty(fh))
                Console.WriteLine("Fehler: " + fh);
            }
          }

          Console.WriteLine("");
          Console.WriteLine("Insgesamt {0} Nachrichten", msgIds.Count);
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
        List<MessageIdent> msgIds;

        Console.WriteLine("Folgende Nachrichten werden gespeichert:");

        var messageSource = new MsgSrcEgvpEpWebservice();
        msgIds = messageSource.getNewMessages(PostboxId);

        if (null != msgIds && msgIds.Count > 0)
        {
          foreach (var msg in msgIds)
          {
            Console.WriteLine(msg.MessageId);

            if (!readOnly)
            {
              Message message = null;
              try
              {
                message = messageSource.ReceiveMessage(msg);
                string zipTempFilename = Path.Combine(tempdir, message.header.MessageId + ".zip");
                string fullfilename = Path.Combine(exportPath, message.header.MessageId);
                if (!Directory.Exists(fullfilename))
                {
                  File.WriteAllBytes(zipTempFilename, message.MessageData);
                  ZipFile.ExtractToDirectory(zipTempFilename, fullfilename);
                  File.Delete(zipTempFilename);
                }
              }
              catch (Exception ex)
              { Console.WriteLine("Fehler: " + ex.Message); }
            }
          }

          Console.WriteLine("");
          Console.WriteLine("Insgesamt {0} Nachrichten", msgIds.Count);
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine("Unbekannter Fehler: {0}", ex.Message.ToString());
      }
    }

    private void ExtractFiles(Message message, LogEntry logEntry)
    {
      string zipFullFilename = Path.Combine(_tempDir, message.header.MessageId + ".zip");
      string fullfilename = "";

      try
      {
        logEntry.AddSubEntry(String.Format("Nachricht temporär zwischenspeichern nach {0}", zipFullFilename), LogEventLevel.Information);
        File.WriteAllBytes(zipFullFilename, message.MessageData);

        List<string> exportPaths;
        List<string> archivPaths;
        if (DepartmentsService.DepartmentsModeActive(this._egvpPostbox))
        {
          logEntry.AddSubEntry("Prüfung Empfängergerichte in xjustiz.xml", LogEventLevel.Information);
          var depService = new DepartmentsService(this._egvpPostbox, message.MessageData);
          depService.ValidateDepartmentSettings(logEntry);
          depService.DetermineDestinations(out exportPaths, out archivPaths);
        }
        else
        {
          exportPaths = XJustizService.IsEeb(message.MessageData) ? this._egvpPostbox.ExportPath_EEB : this._egvpPostbox.ExportPath;
          archivPaths = this._egvpPostbox.ArchivPath;
        }

        foreach (string expPath in exportPaths)
        {
          fullfilename = Path.Combine(expPath, message.header.MessageId);
          CheckAndFixAttachmentFails(zipFullFilename, fullfilename, logEntry);
          logEntry.AddSubEntry(String.Format("Nachricht exportieren nach {0}", fullfilename), LogEventLevel.Information);
          ZipFile.ExtractToDirectory(zipFullFilename, fullfilename);
        }

        try
        {
          foreach (string archPath in archivPaths)
          {
            fullfilename = Path.Combine(DatetimeHelper.ReplaceDatetimeTags(archPath, DateTime.Now), message.header.MessageId);
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
      string illigalFileChars = @"[/:*?""<>|]";

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
              if (System.Text.RegularExpressions.Regex.IsMatch(entry.Name, illigalFileChars))
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
                if (System.Text.RegularExpressions.Regex.IsMatch(entry.FullName, illigalFileChars))
                {
                  newName = System.Text.RegularExpressions.Regex.Replace(oldName, illigalFileChars, "_");
                  logEntry.AddSubEntry(String.Format("Der Dateiname der Anlage '{0}' enthält ungültige Zeichen, es erfolgt eine Umbenennung zu {1}", oldName, newName), LogEventLevel.Warning);
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

    private void CommitMessage(Message message, LogEntry logEntry)
    {
      string warningText = _messageSource.CommitMessage(message);
      if (!string.IsNullOrEmpty(warningText))
        logEntry.AddSubEntry(warningText, LogEventLevel.Warning);  // TODO: refactor
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