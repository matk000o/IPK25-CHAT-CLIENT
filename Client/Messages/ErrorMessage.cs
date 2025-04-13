using Client.Enums;

namespace Client.Messages;

public class ErrorMessage(string sender, string errorContent, string rawText) : IMessage
{
    public MessageType Type => MessageType.Error;
    public string RawText { get; } = rawText;
    public string Sender { get; } = sender;
    public string ErrorContent { get; } = errorContent;
}