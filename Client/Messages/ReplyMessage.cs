using Client.Enums;

namespace Client.Messages;

public class ReplyMessage(bool success, string content, string rawText) : IMessage
{
    public MessageType Type => MessageType.Reply;
    public string RawText { get; } = rawText;
    public bool Success { get; } = success;
    public string Content { get; } = content;
}