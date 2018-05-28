using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OvgRlp.EgvpEpFetcher.Infrastructure.Models
{
    public class LogMetadata
    {
        public string MessageId;
        public string Recipient;
        public string RecipientName;
        public string Sender;
        public string SenderName;
        public string Subject;
        public string MessageType;
        public string MessageSizeKB;
        public string AppVersion;
    }
}