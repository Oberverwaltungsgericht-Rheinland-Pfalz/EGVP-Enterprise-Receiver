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
            configService.GetCommonValue("testprop");
            Console.ReadLine();
        }
    }
}