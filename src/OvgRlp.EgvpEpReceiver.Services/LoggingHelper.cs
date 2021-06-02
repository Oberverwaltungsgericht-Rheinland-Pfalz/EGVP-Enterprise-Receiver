using OvgRlp.Libs.Logging;
using OvgRlp.Libs.Logging.LogTypes;
using System;
using System.IO;

namespace OvgRlp.EgvpEpReceiver.Services
{
  public class LoggingHelper
  {
    public static void AddInnerExceptionToLogEntry(LogEntry le, Exception ex, LogEventLevel loglevel = LogEventLevel.Error)
    {
      if (null != ex.InnerException)
      {
        Exception exx = ex.InnerException;
        do
        {
          le.AddSubEntry(String.Format("erweiterte Fehlerbeschreibung: {0}", exx.Message), loglevel);
          exx = exx.InnerException;
        } while (null != exx);
      }
    }

    public static void InitLogging(string logDir, string conString, string logdt)
    {
      //database logging
      if (!String.IsNullOrWhiteSpace(logdt))
      {
        var ltype = new LogTypeMSSqlServer(logdt);

        if (!string.IsNullOrEmpty(conString))
        {
          var sqlbuilder = new System.Data.SqlClient.SqlConnectionStringBuilder(conString);
          ltype.DBServer = sqlbuilder.DataSource;
          ltype.Database = sqlbuilder.InitialCatalog;
        }

        Logger.LoggingTypes.Add(ltype);
      }

      // Textfile logging
      if (!String.IsNullOrWhiteSpace(logDir))
      {
        LogTypeFile logType;
        if (!Directory.Exists(logDir) || !File.GetAttributes(logDir).HasFlag(FileAttributes.Directory))
        {
          logType = new LogTypeFile(Path.GetDirectoryName(logDir), Path.GetFileName(logDir));
        }
        else
        {
          logType = new LogTypeFile(logDir);
        }
        Logger.LoggingTypes.Add(logType);
      }

      // Log to console
      Logger.LoggingTypes.Add(new LogTypeConsole());
    }
  }
}