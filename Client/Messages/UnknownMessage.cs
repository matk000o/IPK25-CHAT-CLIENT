using Client.Enums;

namespace Client.Messages;

public class UnknownMessage(ushort messageId = 0) : IMessage
{
    public MessageType Type => MessageType.Unknown;
    public ushort MessageId { get; } = messageId;
}