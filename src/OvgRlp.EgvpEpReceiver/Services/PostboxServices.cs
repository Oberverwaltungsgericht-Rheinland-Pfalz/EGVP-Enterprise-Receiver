using OvgRlp.EgvpEpReceiver.EgvpEnterpriseSoap;
using OvgRlp.EgvpEpReceiver.Infrastructure.Models;
using System.Collections.Generic;

namespace OvgRlp.EgvpEpReceiver.Services
{
  public class PostboxServices
  {
    private static EgvpPortTypeClient EgvpClient = new EgvpEnterpriseSoap.EgvpPortTypeClient();

    public static EgvpPostbox GetPostboxParamsFromId(string Id, EgvpPostbox postbox = null)
    {
      var requ = new searchReceiverRequest();
      var resp = new searchReceiverResponse();

      if (null == postbox)
        postbox = new EgvpPostbox();

      // Suche nach der eigenen ID starten
      requ.userID = Id;
      requ.searchCriteria = new EgvpEnterpriseSoap.BusinessCardType()
      {
        userID = new BCItem() { Value = Id }
      };
      resp = EgvpClient.searchReceiver(requ);

      // Auswerten ob ein eindeutiges Postfach gefunden werden konnte
      if (resp.count == 0)
        throw new KeyNotFoundException("In Egvp-Enterprise konnte kein Postfach zu '" + Id + "' ermittelt werden!");
      if (resp.count > 1)
        throw new KeyNotFoundException("In Egvp-Enterprise konnte kein eindeutiges Postfach zu '" + Id + "' ermittelt werden! (Anzahl:" + resp.count.ToString() + ")");

      // Model mit Postfachdaten füllen
      postbox.Id = Id;
      postbox.Name = resp.receiverResults[0].name.Value;

      return postbox;
    }
  }
}