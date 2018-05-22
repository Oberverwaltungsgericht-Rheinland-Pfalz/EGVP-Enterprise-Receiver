using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace OvgRlp.EgvpEpFetcher.Services
{
    public class ConfigurationService : ConfigurationServiceBase
    {
        protected override string XPATH_Common
        {
            get { return "egvpepfetcher/common"; }
        }

        public ConfigurationService(XmlDocument xmlDocument) : base(xmlDocument) { }
    }
}