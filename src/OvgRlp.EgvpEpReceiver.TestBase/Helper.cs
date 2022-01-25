using OvgRlp.EgvpEpReceiver.Infrastructure.EgvpEnterpriseSoap;
using OvgRlp.EgvpEpReceiver.Infrastructure.Models;
using OvgRlp.Libs.Logging;
using OvgRlp.Libs.Logging.LogTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OvgRlp.EgvpEpReceiver.Test
{
  public class Helper
  {
    public static string GetTestingPath()
    {
      var dirPath = getExecutionDirectory();
      string relPath = Path.Combine(dirPath, @"..\..\..\..\..\testing\");
      return Path.GetFullPath(relPath);
    }

    public static string GetTestLogConfigPath()
    {
      return Path.Combine(GetResourcesPath(), "SerilogConfig\\ConsoleOnly.json");
    }

    public static string GetResourcesPath()
    {
      var dirPath = getExecutionDirectory();
      string relPath = Path.Combine(dirPath, @"..\..\..\..\", System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, "Resources");
      return Path.GetFullPath(relPath);
    }

    public static byte[] GetMessageAsZip(string messageName)
    {
      byte[] orig = null;
      string filePath = Path.Combine(GetResourcesPath(), "Messages", messageName);

      if (File.GetAttributes(filePath).HasFlag(FileAttributes.Directory))
      {
        string tmpFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".zip");
        ZipFile.CreateFromDirectory(filePath, tmpFile, CompressionLevel.Fastest, false);
        orig = File.ReadAllBytes(tmpFile);
        File.Delete(tmpFile);
      }

      return orig;
    }

    private static string getExecutionDirectory()
    {
      string dir = "";
      //.NET CORE Projekt:
      /*
      var codeBaseUrl = new Uri(System.Reflection.Assembly.GetExecutingAssembly().Location);
      var codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
      dir = Path.GetDirectoryName(codeBasePath);
      */
      //in .NET Framework Projekten:
      dir = System.AppDomain.CurrentDomain.BaseDirectory;
      return dir;
    }

    public static EgvpPostbox GetEgvpPostbox(string tempPath, string receiverId)
    {
      string archivePath = Path.Combine(tempPath, "Archive");
      string exportPath = Path.Combine(tempPath, "Export");
      string exportPath_EEB = Path.Combine(tempPath, "Export_EEB");

      foreach (string dir in new List<string> { archivePath, exportPath, exportPath_EEB })
      {
        if (Directory.Exists(dir))
          Directory.Delete(dir, true);
        Directory.CreateDirectory(Path.Combine(Helper.GetTestingPath(), dir));
      }

      return new EgvpPostbox
      {
        ArchivPath = new List<string> { archivePath },
        ExportPath = new List<string> { exportPath },
        ExportPath_EEB = new List<string> { exportPath_EEB },
        Id = receiverId,
        IsDisabled = false,
        Name = "TestPostfach",
        ReceiveDepartments = null
      };
    }

    public static string PrepareTempDir(string basePath)
    {
      string dir = Path.Combine(basePath, "Temp");
      if (Directory.Exists(dir))
        Directory.Delete(dir, true);
      Directory.CreateDirectory(dir);
      return dir;
    }

    public static void InitTestLogger(string logFile)
    {
      LogTypeFile logType;
      if (File.Exists(logFile))
        File.Delete(logFile);
      logType = new LogTypeFile(Path.GetDirectoryName(logFile), Path.GetFileName(logFile));
      Logger.LoggingTypes.Add(logType);
    }
  }
}