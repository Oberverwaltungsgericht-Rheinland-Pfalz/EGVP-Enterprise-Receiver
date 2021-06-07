using OvgRlp.EgvpEpReceiver.Infrastructure.Models;
using OvgRlp.Libs.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace OvgRlp.EgvpEpReceiver.Services
{
  public class DepartmentsService
  {
    private protected byte[] _messageZIP;
    private protected EgvpPostbox _egvpPostbox;
    private Lazy<string> XjustizXmlText;
    private Lazy<bool> IsEeb;

    public DepartmentsService(EgvpPostbox egvpPostbox, byte[] messageZIP)
    {
      _egvpPostbox = egvpPostbox;
      _messageZIP = messageZIP;
      XjustizXmlText = new Lazy<string>(() => XJustizService.GetXjustizXmlTextFromZipArchive(_messageZIP));
      IsEeb = new Lazy<bool>(() => XJustizService.IsEeb(XjustizXmlText.Value));
    }

    public void DetermineDestinations(out List<string> exportPaths, out List<string> archivPaths)
    {
      exportPaths = IsEeb.Value ? _egvpPostbox.ExportPath_EEB : _egvpPostbox.ExportPath;
      archivPaths = _egvpPostbox.ArchivPath;

      if (DepartmentsModeActive(_egvpPostbox))
      {
        Department dep = GetDepartment();
        if (null != dep)
        {
          exportPaths = IsEeb.Value ? dep.ExportPath_EEB : dep.ExportPath;
          archivPaths = dep.ArchivPath;
        }
      }
    }

    public Department GetDepartment()
    {
      Department dep = null;
      if (DepartmentsModeActive(_egvpPostbox))
      {
        string depId = GetDepartmentId();
        dep = _egvpPostbox.ReceiveDepartments.Departments.Where(d => d.Id == depId).FirstOrDefault() ?? null;
      }
      return dep;
    }

    public string GetDepartmentId()
    {
      return GetDepartmentValueByXjustizXmlText(_egvpPostbox.ReceiveDepartments.XPathDepartmentId);
    }

    public string GetDepartmentValueByXjustizXmlText(string xpath)
    {
      string rval = string.Empty;
      if (!String.IsNullOrEmpty(XjustizXmlText.Value))
      {
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(XjustizXmlText.Value);

        XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
        nsmgr.AddNamespace("xjustiz", "http://www.xjustiz.de");

        XmlNode xmlNode = doc.SelectSingleNode(xpath, nsmgr);
        if (xmlNode != null)
          rval = xmlNode.InnerXml;
      }
      return rval;
    }

    public void ValidateDepartmentSettings(LogEntry logEntry)
    {
      if (DepartmentsModeActive(_egvpPostbox))
      {
        Department dep = GetDepartment();
        if (null == dep)
        {
          string depId = GetDepartmentId();
          string info = string.Format("Gerichtsempfänger '{0}' konnte nicht in der xjustiz unter '{1}' gefunden werden!", depId, _egvpPostbox.ReceiveDepartments.XPathDepartmentId);
          if (_egvpPostbox.ReceiveDepartments.LogLevelDepartmentNotFound.ToLower() == "warning")
            logEntry.AddSubEntry(info, LogEventLevel.Warning);
          if (_egvpPostbox.ReceiveDepartments.LogLevelDepartmentNotFound.ToLower() == "error")
            throw new Exception(info);
        }
      }
    }

    public static bool DepartmentsModeActive(EgvpPostbox egvpPostbox)
    {
      return (null != egvpPostbox.ReceiveDepartments && null != egvpPostbox.ReceiveDepartments.Departments && egvpPostbox.ReceiveDepartments.Departments.Count >= 1);
    }
  }
}