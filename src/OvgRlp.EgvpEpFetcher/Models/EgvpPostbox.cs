using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OvgRlp.EgvpEpFetcher.Models
{
    public class EgvpPostbox
    {
        public String Id;

        public String Name;

        public List<String> ExportPath;

        public EgvpPostbox()
        {
            //Defaults
            this.ExportPath = new List<string>();
        }
    }
}