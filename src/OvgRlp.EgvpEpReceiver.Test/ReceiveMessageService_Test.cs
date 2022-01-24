using OvgRlp.EgvpEpReceiver.Infrastructure.EgvpEnterpriseSoap;
using OvgRlp.EgvpEpReceiver.Infrastructure.Models;
using OvgRlp.EgvpEpReceiver.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace OvgRlp.EgvpEpReceiver.Test
{
  // TODO: weitere Refaktorisierungen, analog zu MessageSource sollte es auch ein MessageTarget geben - in der Folge können für den Empfang auch kleinteiligere Tests erfolgen
  public class ReceiveMessageService_Test
  {
    public const string MESSAGEID = "govapp_1638957835003741065649003093795";
    public const string SENDERID = "DE.Justiz.5d5d58af-adff-55b7-875c-6548acd55b74.2222";
    public const string RECEIVERID = "DE.Justiz.568b4987-2701-4c8d-8a56-abb554fad842.1111";
    public const string OSCISTATE = "WS_RECEIVED";
    public const string OSCIDATE = "27.01.2022 08:55";

    public const string TMP_PATH = @"tmp\unittest\";
    public const string LOGFILE = TMP_PATH + "Logfile.log";

    [Fact]
    public void Message_Receive_OK()
    {
      string logfile = Path.Combine(Helper.GetTestingPath(), LOGFILE);
      string testMessagePath = Path.Combine(Helper.GetResourcesPath(), "Messages", "Testnachricht_xjustiz321");

      EgvpPostbox postbox = Helper.GetEgvpPostbox(Path.Combine(Helper.GetTestingPath(), TMP_PATH), RECEIVERID);

      var msgSrc = new MsgSrcTst(testMessagePath, MESSAGEID, SENDERID, RECEIVERID, OSCISTATE, OSCIDATE);
      var receiveService = new ReceiveMessageService(msgSrc, postbox, "", Helper.PrepareTempDir(Path.Combine(Helper.GetTestingPath(), TMP_PATH)), "");

      Helper.InitTestLogger(logfile);
      receiveService.ReceiveMessage(MESSAGEID);
      Assert.True(Directory.Exists(Path.Combine(postbox.ExportPath[0], MESSAGEID)));
      Assert.True(Directory.Exists(Path.Combine(postbox.ArchivPath[0], MESSAGEID)));

      string logText = File.ReadAllText(logfile);
      Debug.WriteLine("--> Debug.WriteLine: " + logText);

      Assert.Contains("Nachricht wurde erfolgreich verarbeitet!", logText);
      Assert.DoesNotContain("ERR", logText);
      Assert.Contains("OsciState\\\":\\\"" + OSCISTATE, logText);
      Assert.Contains("Sender\\\":\\\"" + SENDERID, logText);
      Assert.Contains("Recipient\\\":\\\"" + RECEIVERID, logText);
      Assert.Contains("MessageId\\\":\\\"" + MESSAGEID, logText);
    }
  }
}