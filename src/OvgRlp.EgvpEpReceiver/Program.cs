using OvgRlp.EgvpEpReceiver.Services;
using System;

namespace OvgRlp.EgvpEpReceiver
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

    private static void CommmitAllUncommittedMessages()
    {
      // * ReceiveMessageService.CommmitAllUncommittedMessages("govello-1200493560970-000063718", true);   // vgko
      // * ReceiveMessageService.CommmitAllUncommittedMessages("govello-1200493914899-000063722", true);   // vgnw
      // * ReceiveMessageService.CommmitAllUncommittedMessages("govello-1200493687543-000063720", true);   // vgmz
      //ReceiveMessageService.CommmitAllUncommittedMessages("govello-1200494005637-000063724", true);   // vgtr
      //ReceiveMessageService.CommmitAllUncommittedMessages("govello-1200494948121-000063739", true);   // FG

#if DEBUG
      Console.WriteLine("\n\n\nBitte Taste drücken...");
      Console.ReadKey();
#endif
    }
  }
}