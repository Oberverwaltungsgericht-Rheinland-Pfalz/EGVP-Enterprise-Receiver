using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OvgRlp.EgvpEpReceiver.Services
{
  public class XJustizService
  {
    public static bool IsEeb(byte[] messageZIP)
    {
      return IsEeb(GetXjustizXmlTextFromZipArchive(messageZIP));
    }

    public static bool IsEeb(string xjustizXmlText)
    {
      return (!string.IsNullOrEmpty(xjustizXmlText) && xjustizXmlText.Contains("nachricht.eeb.zuruecklaufend"));
    }

    public static string GetXjustizXmlTextFromZipArchive(byte[] messageZIP)
    {
      string rval = string.Empty;
      using (ZipArchive za = new ZipArchive(new MemoryStream(messageZIP)))
      {
        var entries = za.Entries.Where(ze => Path.GetExtension(ze.Name) == ".xml" && Path.GetDirectoryName(ze.FullName) == "attachments" && ze.Name.ToLower().Contains("xjustiz")).ToList();
        if (null != entries && entries.Count > 0)
        {
          TextReader tr = new StreamReader(entries[0].Open());
          rval = tr.ReadToEnd();
        }
      }
      return rval;
    }
  }
}