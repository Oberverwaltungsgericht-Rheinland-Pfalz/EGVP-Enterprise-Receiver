using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OvgRlp.EgvpEpFetcher.Infrastructure.Models
{
    public class EGVPMessageProps : ExportMsgType
    {
        public static EGVPMessageProps LoadFromFile(String filename)
        {
            try
            {
                EGVPMessageProps msgProps = null;

                XmlSerializer xs = new XmlSerializer(typeof(EGVPMessageProps));

                using (StreamReader sr = new StreamReader(filename))
                {
                    msgProps = (EGVPMessageProps)xs.Deserialize(sr);
                }

                return msgProps;
            }
            catch (Exception ex)
            {
                throw new Exception("EGVPMsgProps [" + filename + "] konnte nicht geladen werden.", ex);
            }
        }

        public static EGVPMessageProps LoadFromStream(Stream stream)
        {
            try
            {
                EGVPMessageProps msgProps = null;

                XmlSerializer xs = new XmlSerializer(typeof(EGVPMessageProps));
                msgProps = (EGVPMessageProps)xs.Deserialize(stream);

                return msgProps;
            }
            catch (Exception ex)
            {
                throw new Exception("EGVPMsgProps konnte nicht geladen werden.", ex);
            }
        }
    }
}