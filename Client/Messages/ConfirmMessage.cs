using Client.Enums;

namespace Client.Messages;

public class ConfirmMessage(ushort messageId = 0) : IMessage
{
    public MessageType Type => MessageType.Confirm;
    public ushort MessageId { get; } = messageId;
}