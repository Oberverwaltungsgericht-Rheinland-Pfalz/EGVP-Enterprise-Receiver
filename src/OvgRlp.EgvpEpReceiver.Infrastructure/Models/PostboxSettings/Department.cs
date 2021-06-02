using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OvgRlp.EgvpEpReceiver.Infrastructure.Models
{
  public class Department
  {
    public String Id { get; set; }

    public String Name { get; set; }

    public List<String> ExportPath { get; set; }

    public List<String> ExportPath_EEB { get; set; }

    public List<String> ArchivPath { get; set; }

    public Department()
    {
      //Defaults
      this.ExportPath = new List<string>();
      this.ArchivPath = new List<string>();
      this.ExportPath_EEB = new List<string>();
    }
  }
}