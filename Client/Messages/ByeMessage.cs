using Client.Enums;

namespace Client.Messages;

public class ByeMessage(string sender, ushort messageId = 0) : IMessage
{
    public MessageType Type => MessageType.Bye;
    public ushort MessageId { get; } = messageId;
    public string Sender { get; } = sender;
}