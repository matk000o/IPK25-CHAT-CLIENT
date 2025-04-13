using Client.Enums;

namespace Client.Messages;

public class ChatMessage(string sender, string messageContent, string rawText) : IMessage
{
    public MessageType Type => MessageType.Chat;
    public string RawText { get; } = rawText;
    public string Sender { get; } = sender;
    public string MessageContent { get; } = messageContent;
}