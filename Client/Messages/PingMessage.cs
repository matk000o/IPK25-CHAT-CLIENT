using Client.Enums;

namespace Client.Messages;

public class PingMessage(ushort messageId = 0) : IMessage
{
    public MessageType Type => MessageType.Ping;
    public ushort MessageId { get; } = messageId;
}