using Client.Enums;

namespace Client.Messages;

public class ErrorMessage(string sender, string errorContent, ushort messageId = 0) : IMessage
{
    public MessageType Type => MessageType.Err;
    public ushort MessageId { get; } = messageId;
    public string Sender { get; } = sender;
    public string ErrorContent { get; } = errorContent;
}