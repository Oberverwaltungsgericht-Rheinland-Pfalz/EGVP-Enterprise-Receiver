using OvgRlp.EgvpEpFetcher.Infrastructure.Models;
using OvgRlp.EgvpEpFetcher.Services;
using OvgRlp.Libs.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OvgRlp.EgvpEpFetcher
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string logKontext = "";

            // Logger initialisieren
            LoggingHelper.InitLogging();

            try
            {
                logKontext = "ConfigurationService initialisieren";
                Console.WriteLine("ConfigurationService wird initialisiert");
                var configService = ConfigurationService.Load<ConfigurationService>(Properties.Settings.Default.configfile);
                logKontext = "Postfächer aus Konfig lesen";
                Console.WriteLine("Postfächer werden aus der Konfig gelesen");
                List<EgvpPostbox> egvpPostBoxes = configService.GetAllPostboxes();

                foreach (EgvpPostbox egvpPostBox in egvpPostBoxes)
                {
                    try
                    {
                        logKontext = "Eingangsverarbeitung für " + egvpPostBox.Name;
                        Console.WriteLine("*** Eingänge für " + egvpPostBox.Name + " verarbeiten ***");
                        var receiveMessageService = new ReceiveMessageService(egvpPostBox);
                        receiveMessageService.ReceiveMessages();
                    }
                    catch (Exception ex)
                    { UnexpectedException(ex, logKontext); }
                }
            }
            catch (Exception ex)
            { UnexpectedException(ex, logKontext); }

#if DEBUG
            Console.WriteLine("\n\n\nBitte Taste drücken...");
            Console.ReadKey();
#endif
        }

        private static void UnexpectedException(Exception ex, string logKontext)
        {
            using (var log = Logger.CreateLogger())
            {
                var logEntry = new LogEntry(String.Format("Unerwarteter Fehler - Ausnahme:  {0}", ex.Message), LogEventLevel.Fatal);
                LoggingHelper.AddInnerExceptionToLogEntry(logEntry, ex);
                Logger.Log(logEntry, logKontext);
            }
        }
    }
}