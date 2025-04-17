using Client.Enums;

namespace Client.Messages;

public class ChatMessage(string sender, string messageContent, ushort messageId = 0) : IMessage
{
    public MessageType Type => MessageType.Msg;
    public ushort MessageId { get; } = messageId;
    public string Sender { get; } = sender;
    public string MessageContent { get; } = messageContent;
}