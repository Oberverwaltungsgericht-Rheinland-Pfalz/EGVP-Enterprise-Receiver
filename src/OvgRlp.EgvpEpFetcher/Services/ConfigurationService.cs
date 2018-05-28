using OvgRlp.EgvpEpFetcher.Infrastructure.Models;
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
            string debugKontext = "";

            XmlNodeList PostboxIDs = xmlDocument.SelectNodes(XPATH_Postboxes + "/" + TAG_Postbox);
            foreach (XmlNode pb in PostboxIDs)
            {
                try
                {
                    if (null != pb.Attributes["Id"])
                    {
                        var id = pb.Attributes["Id"].Value;
                        debugKontext = id;
                        EgvpPostbox epb = PostboxServices.GetPostboxParamsFromId(id);

                        XmlNodeList exports = pb.SelectNodes("export/path");
                        foreach (XmlNode exp in exports)
                            epb.ExportPath.Add(exp.InnerText);

                        if (null == postboxes)
                            postboxes = new List<EgvpPostbox>();
                        postboxes.Add(epb);
                    }
                }
                catch (Exception ex)
                {
                    //nur in die Console Loggen, ansonsten würden die Logs zu unübersichtlich
                    Console.WriteLine(String.Format("### FEHLER ###\nBeim lesen von Postfächern aus der Konfig ({0}):\n{1}", debugKontext, ex.Message));
                }
            }

            return postboxes;
        }

        public EgvpPostbox GetPostbox(string Id)
        {
            EgvpPostbox egvpPostbox = null;
            List<EgvpPostbox> egvpPostBoxes = this.GetAllPostboxes();

            if (null != egvpPostBoxes)
            {
                var foo = egvpPostBoxes.Where(pb => pb.Id == Id);
                if (null != foo && foo.Count() > 0)
                    egvpPostbox = foo.First();
            }

            return egvpPostbox;
        }
    }
}