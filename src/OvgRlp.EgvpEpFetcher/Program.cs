using OvgRlp.EgvpEpFetcher.Services;
using System;

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