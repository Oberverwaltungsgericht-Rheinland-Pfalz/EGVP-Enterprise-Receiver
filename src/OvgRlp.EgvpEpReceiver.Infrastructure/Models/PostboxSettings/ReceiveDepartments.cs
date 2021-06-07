using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OvgRlp.EgvpEpReceiver.Infrastructure.Models
{
  public class ReceiveDepartments
  {
    public string LogLevelDepartmentNotFound { get; set; }

    public string XPathDepartmentId { get; set; }

    public List<Department> Departments { get; set; }

    public ReceiveDepartments()
    {
      //Defaults
      this.Departments = new List<Department>();
    }
  }
}