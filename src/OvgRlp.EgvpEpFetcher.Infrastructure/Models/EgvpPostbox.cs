using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OvgRlp.EgvpEpFetcher.Infrastructure.Models
{
  public class EgvpPostbox
  {
    public String Id;

    public String Name;

    public List<String> ExportPath;

    public List<String> ExportPath_EEB;

    public List<String> ArchivPath;

    public EgvpPostbox()
    {
      //Defaults
      this.ExportPath = new List<string>();
      this.ArchivPath = new List<string>();
      this.ExportPath_EEB = new List<string>();
    }
  }
}