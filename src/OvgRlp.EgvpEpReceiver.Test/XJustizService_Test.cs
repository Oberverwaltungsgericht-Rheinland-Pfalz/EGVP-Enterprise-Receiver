using OvgRlp.EgvpEpReceiver.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace OvgRlp.EgvpEpReceiver.Test
{
  public class XJustizService_Test
  {
    [Fact]
    public void Message_DetermineXJustizText_OK()
    {
      byte[] message = Helper.GetMessageAsZip("Testnachricht_1");
      Assert.Contains("http://www.xjustiz.de", XJustizService.GetXjustizXmlTextFromZipArchive(message));
    }

    [Fact]
    public void Message_IsNotEeb()
    {
      byte[] message = Helper.GetMessageAsZip("Testnachricht_1");
      Assert.False(XJustizService.IsEeb(message));
    }

    [Fact]
    public void Message_IsEeb()
    {
      byte[] message = Helper.GetMessageAsZip("TestnachrichtEeb_1");
      Assert.True(XJustizService.IsEeb(message));
    }
  }
}