using OvgRlp.EgvpEpFetcher.Models;
using OvgRlp.EgvpEpFetcher.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OvgRlp.EgvpEpFetcher
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var configService = ConfigurationService.Load<ConfigurationService>(Properties.Settings.Default.configfile);
            List<EgvpPostbox> egvpPostBoxes = configService.GetAllPostboxes();

            foreach (EgvpPostbox egvpPostBox in egvpPostBoxes)
            {
                Console.WriteLine(egvpPostBox.Name);
                foreach (string exp in egvpPostBox.ExportPath)
                    Console.WriteLine(exp);

                var receiveMessageService = new ReceiveMessageService(egvpPostBox);
                receiveMessageService.ReceiveMessages();
            }

            Console.ReadLine();
        }
    }
}