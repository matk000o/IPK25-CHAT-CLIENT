using Client.Enums;

namespace Client.Messages;

public class ByeMessage(string sender, string rawText) : IMessage
{
    public MessageType Type => MessageType.Bye;
    public string RawText { get; } = rawText;
    public string Sender { get; } = sender;
}