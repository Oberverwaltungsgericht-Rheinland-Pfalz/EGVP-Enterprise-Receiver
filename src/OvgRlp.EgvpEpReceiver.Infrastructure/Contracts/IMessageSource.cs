using OvgRlp.EgvpEpReceiver.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OvgRlp.EgvpEpReceiver.Infrastructure.Contracts
{
  public interface IMessageSource
  {
    List<MessageIdent> getNewMessages(string receiverId, string receiverName);

    Message ReceiveMessage(MessageIdent messageIdent);

    string CommitMessage(Message message);    // TODO: refactor - eleminate string callback
  }
}