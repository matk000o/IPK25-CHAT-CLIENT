using Client.Enums;

namespace Client.Messages;

public class ReplyMessage(bool success, string content, ushort messageId = 0, ushort referenceMessageId = 0) : IMessage
{
    public MessageType Type => MessageType.Reply;
    public ushort MessageId { get; } = messageId;
    public ushort ReferenceMessageId { get; } = referenceMessageId;

    public bool Success { get; } = success;
    public string Content { get; } = content;
}