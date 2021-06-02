using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OvgRlp.EgvpEpReceiver.Infrastructure.Models
{
  public class ReceiveDepartment
  {
    public String Id;

    public String Name;

    public List<String> ExportPath;

    public List<String> ExportPath_EEB;

    public List<String> ArchivPath;

    public ReceiveDepartment()
    {
      //Defaults
      this.ExportPath = new List<string>();
      this.ArchivPath = new List<string>();
      this.ExportPath_EEB = new List<string>();
    }
  }
}