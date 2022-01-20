using OvgRlp.EgvpEpReceiver.Infrastructure.Contracts;
using OvgRlp.EgvpEpReceiver.Infrastructure.EgvpEnterpriseSoap;
using OvgRlp.EgvpEpReceiver.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OvgRlp.EgvpEpReceiver.Services
{
  public class MsgSrcEgvpEpWebservice : IMessageSource
  {
    public static EgvpPortTypeClient EgvpClient = new EgvpPortTypeClient();

    public List<MessageIdent> getNewMessages(string receiverId, string receiverName = "")
    {
      var msgIdents = new List<MessageIdent>();

      var requ = new getUncommittedMessageIDsRequest();
      requ.userID = receiverId;
      requ.userID = receiverId;

      getUncommittedMessageIDsResponse resp = EgvpClient.getUncommittedMessageIDs(requ);
      if (resp.returnCode != GetUncommittedMessageIDsReturnCodeType.OK)
        throw new Exception(string.Format("Fehler bei getUncommittedMessageIDs im Postfach {0}: {1}", receiverName, resp.returnCode.ToString()));

      if (null != resp.uncommittedMessages)
      {
        foreach (var msg in resp.uncommittedMessages)
        {
          msgIdents.Add(new MessageIdent { MessageId = msg.messageID, ReceiverId = receiverId });
        }
      }

      return msgIdents;
    }

    public string CommitMessage(Message message)
    {
      string errorText = "";
      var requ = new commitReceivedMessageRequest();
      requ.messageID = message.header.MessageId;
      requ.userID = message.header.ReceiverId;

      commitReceivedMessageResponse resp = EgvpClient.commitReceivedMessage(requ);
      if (resp.returnCode != CommitReturnCodeType.OK)
      {
        string ft = string.Format("Fehler bei commitReceivedMessage - ID {0}: {1}", message.header.MessageId, resp.returnCode.ToString());
        if (resp.returnCode != CommitReturnCodeType.MESSAGE_ALREADY_COMMITTED)
        { throw new Exception(ft); }
        else
        { errorText = ft; }
      }
      return errorText;
    }

    public Message ReceiveMessage(MessageIdent messageIdent)
    {
      var message = new Message();

      var requ = new receiveMessageRequest();
      requ.messageID = messageIdent.MessageId;
      requ.userID = messageIdent.ReceiverId;

      receiveMessageResponse resp = EgvpClient.receiveMessage(requ);
      if (resp.returnCode != ReceiveReturnCodeType.OK)
        throw new Exception(string.Format("Fehler bei receiveMessage - ID {0}: {1}", messageIdent.MessageId, resp.returnCode.ToString()));
      if (null == resp || null == resp.messageZIP)
        throw new Exception("Fehler bei receiveMessage - resp.messageZIP ist null");

      message.header = messageIdent;
      message.MessageData = resp.messageZIP;
      return message;
    }
  }
}