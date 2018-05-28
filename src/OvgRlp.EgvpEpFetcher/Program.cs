using OvgRlp.EgvpEpFetcher.Infrastructure.Models;
using OvgRlp.EgvpEpFetcher.Services;
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
            var cliService = new CLIService();
            CLIActions cliActions = cliService.ParseCommandLineArguments(args);
            if (null != cliActions)
                cliActions.ExecuteActions();
#if DEBUG
            Console.WriteLine("\n\n\nBitte Taste drücken...");
            Console.ReadKey();
#endif
        }
    }
}