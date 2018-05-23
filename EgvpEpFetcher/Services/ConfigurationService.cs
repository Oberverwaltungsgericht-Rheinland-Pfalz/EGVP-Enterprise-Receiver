using OvgRlp.EgvpEpFetcher.Models;
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
        private const string XPATH_Postboxes = "egvpepfetcher/postboxes";
        private const string TAG_Postbox = "postbox";

        protected override string XPATH_Common
        {
            get { return "egvpepfetcher/common"; }
        }

        public ConfigurationService(XmlDocument xmlDocument) : base(xmlDocument)
        {
        }

        public List<EgvpPostbox> GetAllPostboxes()
        {
            List<EgvpPostbox> postboxes = null;

            XmlNodeList PostboxIDs = xmlDocument.SelectNodes(XPATH_Postboxes + "/" + TAG_Postbox);
            foreach (XmlNode pb in PostboxIDs)
            {
                if (null != pb.Attributes["Id"])
                {
                    var id = pb.Attributes["Id"].Value;
                    EgvpPostbox epb = PostboxServices.GetPostboxParamsFromId(id);

                    XmlNodeList exports = pb.SelectNodes("export/path");
                    foreach (XmlNode exp in exports)
                        epb.ExportPath.Add(exp.InnerText);

                    if (null == postboxes)
                        postboxes = new List<EgvpPostbox>();
                    postboxes.Add(epb);
                }
            }

            return postboxes;
        }
    }
}