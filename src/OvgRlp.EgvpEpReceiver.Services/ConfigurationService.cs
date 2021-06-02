using OvgRlp.Core.Services;
using OvgRlp.EgvpEpReceiver.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace OvgRlp.EgvpEpReceiver.Services
{
  public class ConfigurationService : ConfigurationBase
  {
    private const string XPATH_Postboxes = "egvpepreceiver/postboxes";
    private const string TAG_Postbox = "postbox";

    protected override string XPATH_Common
    {
      get { return "egvpepreceiver/common"; }
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

            XmlNodeList exportsEeb = pb.SelectNodes("exportEeb/path");
            foreach (XmlNode exp in exportsEeb)
              epb.ExportPath_EEB.Add(exp.InnerText);

            XmlNodeList archiv = pb.SelectNodes("archiv/path");
            foreach (XmlNode arch in archiv)
              epb.ArchivPath.Add(arch.InnerText);

            if (null == postboxes)
              postboxes = new List<EgvpPostbox>();
            postboxes.Add(epb);
          }
        }
        catch (KeyNotFoundException ex)
        {
          //nur in die Console Loggen, ansonsten würden die Logs zu unübersichtlich
          Console.WriteLine(String.Format("### FEHLER ###\nBeim lesen von Postfächern aus der Konfig ({0}):\n{1}", debugKontext, ex.Message));
        }
        catch (Exception ex)
        {
          throw ex;
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