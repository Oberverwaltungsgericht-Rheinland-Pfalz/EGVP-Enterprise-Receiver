using OvgRlp.EgvpEpReceiver.Infrastructure.Contracts;
using OvgRlp.EgvpEpReceiver.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OvgRlp.EgvpEpReceiver.Test
{
  public class MsgSrcTst : IMessageSource
  {
    protected string _messageFullPath;
    protected string _messageId;
    protected string _senderId;
    protected string _receiverId;
    protected string _messageState;
    protected string _messageDatetime;

    public MsgSrcTst(string msgFullPath, string messageId, string senderId, string receiverId, string messageState, string messageDatetime)
    {
      _messageFullPath = msgFullPath;
      _messageId = messageId;
      _senderId = senderId;
      _receiverId = receiverId;
      _messageState = messageState;
      _messageDatetime = messageDatetime;
    }

    public List<MessageIdent> getNewMessages(string receiverId, string receiverName = "")
    {
      var msgIdents = new List<MessageIdent>();
      msgIdents.Add(new MessageIdent { MessageId = _messageId, ReceiverId = _receiverId });
      return msgIdents;
    }

    public string CommitMessage(Message message)
    {
      string errorText = "";

      return errorText;
    }

    public Message ReceiveMessage(MessageIdent messageIdent)
    {
      var message = new Message();

      message.header = messageIdent;

      string tmpFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".zip");
      ZipFile.CreateFromDirectory(_messageFullPath, tmpFile, CompressionLevel.Fastest, false);

      message.MessageData = File.ReadAllBytes(tmpFile);

      File.Delete(tmpFile);

      return message;
    }

    public MessageMetadata GetMessageMetadata(MessageIdent messageIdent)
    {
      var metadata = new MessageMetadata();

      metadata.ReceiverId = _receiverId;
      metadata.SenderId = _senderId;
      metadata.State = _messageState;
      metadata.Datetime = _messageDatetime;

      return metadata;
    }
  }
}