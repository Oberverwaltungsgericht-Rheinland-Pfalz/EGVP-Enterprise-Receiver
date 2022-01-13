using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OvgRlp.EgvpEpReceiver.Test
{
  internal class Helper
  {
    public static string GetTestingPath()
    {
      var codeBaseUrl = new Uri(System.Reflection.Assembly.GetExecutingAssembly().Location);
      var codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
      var dirPath = Path.GetDirectoryName(codeBasePath);
      string relPath = Path.Combine(dirPath, @"..\..\..\..\..\..\testing\");
      return Path.GetFullPath(relPath);
    }

    public static string GetTestLogConfigPath()
    {
      return Path.Combine(GetResourcesPath(), "SerilogConfig\\ConsoleOnly.json");
    }

    public static string GetResourcesPath()
    {
      var codeBaseUrl = new Uri(System.Reflection.Assembly.GetExecutingAssembly().Location);
      var codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
      var dirPath = Path.GetDirectoryName(codeBasePath);
      string relPath = Path.Combine(dirPath, @"..\..\..\Resources\");
      return Path.GetFullPath(relPath);
    }
  }
}