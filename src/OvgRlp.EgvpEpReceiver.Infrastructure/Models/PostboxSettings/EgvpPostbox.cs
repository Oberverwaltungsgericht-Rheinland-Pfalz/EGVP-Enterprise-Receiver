using System;
using System.Collections.Generic;

namespace OvgRlp.EgvpEpReceiver.Infrastructure.Models
{
  public class EgvpPostbox
  {
    public String Id { get; set; }

    public String Name { get; set; }

    public List<String> ExportPath { get; set; }

    public List<String> ExportPath_EEB { get; set; }

    public List<String> ArchivPath { get; set; }

    public ReceiveDepartments ReceiveDepartments { get; set; }

    public EgvpPostbox()
    {
      //Defaults
      this.ExportPath = new List<string>();
      this.ArchivPath = new List<string>();
      this.ExportPath_EEB = new List<string>();
    }
  }
}