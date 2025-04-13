using Client.Enums;

namespace Client.Messages;

public class UnknownMessage(string rawText) : IMessage
{
    public MessageType Type => MessageType.Unknown;
    public string RawText { get; } = rawText;
}