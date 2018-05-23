using OvgRlp.EgvpEpFetcher.EgvpEnterpriseSoap;
using OvgRlp.EgvpEpFetcher.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OvgRlp.EgvpEpFetcher.Services
{
    public class PostboxServices
    {
        private static EgvpPortTypeClient egvpClient = new EgvpEnterpriseSoap.EgvpPortTypeClient();

        public static EgvpPostbox GetPostboxParamsFromId(string Id, EgvpPostbox postbox = null)
        {
            var requ = new searchReceiverRequest();
            var resp = new searchReceiverResponse();

            if (null == postbox)
                postbox = new EgvpPostbox();

            // Suche nach der eigenen ID starten
            requ.userID = Id;
            requ.searchCriteria = new BusinessCardType()
            {
                userID = new BCItem() { Value = Id }
            };
            resp = egvpClient.searchReceiver(requ);

            // Auswerten ob ein eindeutiges Postfach gefunden werden konnte
            if (resp.count == 0)
                throw new KeyNotFoundException("Es konnte kein Postfach zu '" + Id + "' ermittelt werden!");
            if (resp.count > 1)
                throw new KeyNotFoundException("Es konnte kein eindeutiges Postfach zu '" + Id + "' ermittelt werden! (Anzahl:" + resp.count.ToString() + ")");

            // Model mit Postfachdaten füllen
            postbox.Id = Id;
            postbox.Name = resp.receiverResults[0].name.Value;

            return postbox;
        }
    }
}