using OvgRlp.EgvpEpFetcher.EgvpEnterpriseSoap;
using OvgRlp.EgvpEpFetcher.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OvgRlp.EgvpEpFetcher.Services
{
    public class ReceiveMessageService
    {
        private static EgvpPortTypeClient egvpClient = new EgvpEnterpriseSoap.EgvpPortTypeClient();
        public EgvpPostbox EgvpPostbox { get; set; }

        public ReceiveMessageService(EgvpPostbox postbox)
        {
            this.EgvpPostbox = postbox;
        }

        public void ReceiveMessages()
        {
            var requ = new getUncommittedMessageIDsRequest();
            var resp = new getUncommittedMessageIDsResponse();

            if (null == this.EgvpPostbox)
                throw new ArgumentNullException("EgvpPostbox");

            //Uncommitted ID´s
            requ.userID = this.EgvpPostbox.Id;
            resp = egvpClient.getUncommittedMessageIDs(requ);

            foreach (var msg in resp.uncommittedMessages)
            {
                ReceiveMessage(msg.messageID);
            }
        }

        private void ReceiveMessage(string messageId)
        {
            var requ = new receiveMessageRequest();
            var resp = new receiveMessageResponse();

            requ.messageID = messageId;
            requ.userID = this.EgvpPostbox.Id;

            resp = egvpClient.receiveMessage(requ);
            ExtractFiles(resp);
            //CommitMessage(messageId);
        }

        private void ExtractFiles(receiveMessageResponse resp)
        {
            string zipFullFilename = Path.Combine(Properties.Settings.Default.tempDir, resp.messageID + ".zip");
            File.WriteAllBytes(zipFullFilename, resp.messageZIP);
            foreach (string expPath in this.EgvpPostbox.ExportPath)
            {
                ZipFile.ExtractToDirectory(zipFullFilename, Path.Combine(expPath, resp.messageID));
            }
            File.Delete(zipFullFilename);
        }

        private void CommitMessage(string messageId)
        {
            var requ = new commitReceivedMessageRequest();
            var resp = new commitReceivedMessageResponse();

            requ.messageID = messageId;
            requ.userID = this.EgvpPostbox.Id;

            resp = egvpClient.commitReceivedMessage(requ);
        }
    }
}