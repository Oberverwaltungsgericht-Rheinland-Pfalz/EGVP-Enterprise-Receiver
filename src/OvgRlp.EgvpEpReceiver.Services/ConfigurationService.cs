using Microsoft.Extensions.Configuration;
using OvgRlp.Core.Services;
using OvgRlp.EgvpEpReceiver.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace OvgRlp.EgvpEpReceiver.Services
{
  public class ConfigurationService
  {
    public static IConfiguration Configuration { get; set; }

    public ConfigurationService(string configFilename)
    {
      if (string.IsNullOrEmpty(configFilename))
        throw new ArgumentException(this.GetType().Name + " - Pfad zur Json-Config-Datei muss im Konstruktor übergeben werden", nameof(configFilename));

      if (!configFilename.Contains(".json"))
        throw new ArgumentException(this.GetType().Name + " - es werden nur noch json Konfigdateien unterstützt", nameof(configFilename));

      ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
      configurationBuilder.AddJsonFile(configFilename, false, true);
      Configuration = configurationBuilder.Build();
    }

    public List<EgvpPostbox> GetAllPostboxes()
    {
      List<EgvpPostbox> postboxes = null;

      try
      {
        postboxes = Configuration.GetSection("postboxes").Get<List<EgvpPostbox>>();
      }
      catch (Exception ex)
      {
        throw ex;
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