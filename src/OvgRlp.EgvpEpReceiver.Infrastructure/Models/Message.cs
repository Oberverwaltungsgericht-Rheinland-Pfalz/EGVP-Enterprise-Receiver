using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OvgRlp.EgvpEpReceiver.Infrastructure.Models
{
  public class Message
  {
    public MessageIdent header;
    public byte[] MessageData;

    public Message()
    {
      header = new MessageIdent();
    }
  }
}