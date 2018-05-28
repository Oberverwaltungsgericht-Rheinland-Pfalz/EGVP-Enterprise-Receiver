using NDesk.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OvgRlp.EgvpEpFetcher.Services
{
    internal class CLIService
    {
        public CLIService()
        {
            System.Diagnostics.Trace.Listeners.Add(new System.Diagnostics.ConsoleTraceListener());
        }

        //! Kommandozeilenargumente auswerten
        public CLIActions ParseCommandLineArguments(String[] args, bool validate = true)
        {
            var cliActions = new CLIActions();

            var p = new OptionSet();
            p.Add("a|autoimport", "AutoImport anhand des configfiles " + Properties.Settings.Default.configfile, v => cliActions.ImportByConfig = true);
            p.Add("u|userid=", "{EGVP-Postbox-Id}, nur in Verbindung mit Angabe der 'msgid'.", v => cliActions.UserId = v);
            p.Add("m|msgid=", "{EGVP-Message-Id}, nur in Verbindung mit Angabe der 'userid'.", v => cliActions.MessageId = v);
            p.Add("h|?|help", "Hilfe anzeigen.", v => { if (v != null) cliActions.WritingInformationToUser = true; ShowHelp(p); });
            p.Add("v|version", "Versionsinformationen anzeigen.", v => { if (v != null) cliActions.WritingInformationToUser = true; ShowVersionInformation(); });

            try
            {
                p.Parse(args);
                if (validate)
                    cliActions.ValidateActions();
            }
            catch (Exception e)
            {
                Console.Write("EgvpEpFetcher: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Bitte verwenden Sie `EgvpEpFetcher --help' für mehr Informationen.");
                return null;
            }

            return cliActions;
        }

        private static void ShowVersionInformation()
        {
            Console.WriteLine("EgvpEpFetcher Version " + CommonHelper.AssemblyVersion());
        }

        //! Übersicht der möglichen Kommandozeilenargumente auf der Konsole ausgeben
        private static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Verwendung: EgvpEpFetcher [OPTIONEN] + [OPTIONSWERTE]");
            Console.WriteLine();
            Console.WriteLine("OPTIONEN:");
            p.WriteOptionDescriptions(Console.Out);
        }
    }
}