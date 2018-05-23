using OvgRlp.EgvpEpFetcher.EgvpEnterpriseSoap;
using OvgRlp.EgvpEpFetcher.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OvgRlp.EgvpEpFetcher.Services
{
    public class ProtocolService
    {
        public static void CreateLogMetadata(receiveMessageResponse resp, ref LogMetadata logMetadata)
        {
            if (null == logMetadata)
                logMetadata = new LogMetadata();
            logMetadata.AppVersion = CommonHelper.AssemblyVersion();

            if (null != resp)
            {
                /*
                if (!String.IsNullOrEmpty(resp.messageZIP))
                {
                    logMetadata.Aktenzeichen = ebServiceClass.Aktenzeichen;
                }
                else if (!String.IsNullOrEmpty(ebServiceClass.Az))
                {
                    logMetadata.Aktenzeichen = ebServiceClass.Az;
                }
                if (!String.IsNullOrEmpty(ebServiceClass.AdrNr))
                    logMetadata.AdressNr = ebServiceClass.AdrNr;
                if (!String.IsNullOrEmpty(ebServiceClass.EBID))
                    logMetadata.EBID = ebServiceClass.EBID;
                if (!String.IsNullOrEmpty(ebServiceClass.DocPfad))
                    logMetadata.DocPfad = ebServiceClass.DocPfad;
                if (!String.IsNullOrEmpty(ebServiceClass.DirFullName))
                    logMetadata.EBOrdner = Path.GetFileName(ebServiceClass.DirFullName);
                if (!String.IsNullOrEmpty(ebServiceClass.FileFullName))
                    logMetadata.EBDatei = Path.GetFileName(ebServiceClass.FileFullName);
                */
            }
        }
    }
}