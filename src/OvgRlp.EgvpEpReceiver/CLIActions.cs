using OvgRlp.EgvpEpReceiver.Infrastructure.EgvpEnterpriseSoap;
using OvgRlp.EgvpEpReceiver.Infrastructure.Models;
using OvgRlp.Libs.Logging;
using System;
using System.Collections.Generic;

namespace OvgRlp.EgvpEpReceiver.Services
{
  internal class CLIActions
  {
    public bool WritingInformationToUser;
    public bool ImportByConfig;
    public bool ShowMsgState;
    public string MessageId;
    public string UserId;
    private string LogKontext;

    public CLIActions()
    {
      //Defaults
      this.ImportByConfig = true;                 // im Standard Konfig abarbeiten (auch ohne angabe von Parametern)
      this.WritingInformationToUser = false;
      this.LogKontext = "";
    }

    public void ValidateActions()
    {
      if (WritingInformationToUser)
        return;

      if (!String.IsNullOrEmpty(this.MessageId) || !String.IsNullOrEmpty(this.UserId) || this.ShowMsgState)
      {
        string err = "";
        if (String.IsNullOrEmpty(this.MessageId))
          err = "MessageId fehlt!";
        if (String.IsNullOrEmpty(this.UserId))
          err = "UserId fehlt!";
        if (err != "")
          throw new ArgumentException(err + " MessageId und UserId müssen im Verbund angegeben werden!");
        this.ImportByConfig = false;
      }
    }

    public void ExecuteActions()
    {
      if (WritingInformationToUser)
        return;
      ValidateActions();

      // Logger initialisieren
      LoggingHelper.InitLogging(Properties.Settings.Default.LogFile, Properties.Settings.Default.LogDbConnectionString, Properties.Settings.Default.LogDbDatatable);

      //ggf. nur eine bstimmte Nachricht einlesen oder auch nur den Status anzeigen
      if (this.ShowMsgState)
      {
        ShowMessageState();
      }
      else
      {
        if (!String.IsNullOrEmpty(this.MessageId) || !String.IsNullOrEmpty(this.UserId))
        { ImportSingleMessage(); }
        else
        { ImportAllUncommitted(); }
      }
    }

    private void ImportSingleMessage()
    {
      try
      {
        this.LogKontext = "Eingangsverarbeitung für einzelne Nachricht";
        Console.WriteLine("Eingangsverarbeitung für einzelne Nachricht");

        this.LogKontext = string.Format("Postfach {0} aus Konfig lesen", this.UserId);
        Console.WriteLine(string.Format("Postfach {0} aus Konfig lesen", this.UserId));
        var configService = new ConfigurationService(Properties.Settings.Default.configfile);
        EgvpPostbox egvpPostBox = configService.GetPostbox(this.UserId);
        if (null == egvpPostBox)
          throw new ArgumentException("EGVP-Postbox zu Id " + this.UserId + " konnte nicht in der Konfigurationsdatei ermittelt werden", "UserId");

        this.LogKontext = string.Format("MessageId {0} für {1}", this.MessageId, egvpPostBox.Name);
        Console.WriteLine(string.Format("*** MessageId {0} für {1} verarbeiten ***", this.MessageId, egvpPostBox.Name));
        var receiveMessageService = new ReceiveMessageService(new MsgSrcEgvpEpWebservice(), egvpPostBox, Properties.Settings.Default.WaitingHoursAfterError, Properties.Settings.Default.tempDir, Properties.Settings.Default.LockFile);
        receiveMessageService.ReceiveMessage(this.MessageId, true);
      }
      catch (Exception ex)
      { UnexpectedException(ex); }
    }

    private void ImportAllUncommitted()
    {
      try
      {
        var messageSource = new MsgSrcEgvpEpWebservice();
        this.LogKontext = "ConfigurationService initialisieren";
        Console.WriteLine("ConfigurationService wird initialisiert");
        var configService = new ConfigurationService(Properties.Settings.Default.configfile);
        this.LogKontext = "Postfächer aus Konfig lesen";
        Console.WriteLine("Postfächer werden aus der Konfig gelesen");
        List<EgvpPostbox> egvpPostBoxes = configService.GetAllPostboxes();

        foreach (EgvpPostbox egvpPostBox in egvpPostBoxes)
        {
          try
          {
            if (egvpPostBox.IsDisabled)
              continue;
            this.LogKontext = "Eingangsverarbeitung für " + egvpPostBox.Name;
            Console.WriteLine("*** Eingänge für " + egvpPostBox.Name + " verarbeiten ***");
            var receiveMessageService = new ReceiveMessageService(messageSource, egvpPostBox, Properties.Settings.Default.WaitingHoursAfterError, Properties.Settings.Default.tempDir, Properties.Settings.Default.LockFile);
            receiveMessageService.ReceiveMessages();
          }
          catch (Exception ex)
          { UnexpectedException(ex); }
        }
      }
      catch (Exception ex)
      { UnexpectedException(ex); }
    }

    private void ShowMessageState()
    {
      try
      {
        var configService = new ConfigurationService(Properties.Settings.Default.configfile);
        EgvpPostbox egvpPostBox = configService.GetPostbox(this.UserId);
        if (null == egvpPostBox)
          throw new ArgumentException("EGVP-Postbox zu Id " + this.UserId + " konnte nicht in der Konfigurationsdatei ermittelt werden", "UserId");

        var messageSource = new MsgSrcEgvpEpWebservice();
        MessageMetadata msgMeta = messageSource.GetMessageMetadata(new MessageIdent { MessageId = this.MessageId, ReceiverId = this.UserId });

        Console.WriteLine("Osci-Status: " + msgMeta.State);
        Console.WriteLine("Osci-Datum:  " + msgMeta.Datetime);

        // Auf Eingabe warten - bspw. bei Aufruf aus Dashboard
        Console.WriteLine("\n\n\nBitte Taste drücken...");
        Console.ReadKey();
      }
      catch (Exception ex)
      {
        Console.WriteLine("ShowMessageState: Fehler beim Abrufen des Nachrichtenstatus zu Nachricht " + this.MessageId);
        Console.WriteLine("Fehlerbeschreibung: " + ex.Message);
        if (null != ex.InnerException)
          Console.WriteLine("erweiterte Fehlerbeschreibung: " + ex.InnerException.Message);
      }
    }

    private void UnexpectedException(Exception ex)
    {
      using (var log = Logger.CreateLogger())
      {
        var logEntry = new LogEntry(String.Format("Unerwarteter Fehler - Ausnahme:  {0}", ex.Message), LogEventLevel.Fatal);
        LoggingHelper.AddInnerExceptionToLogEntry(logEntry, ex);
        Logger.Log(logEntry, this.LogKontext);
      }
    }
  }
}