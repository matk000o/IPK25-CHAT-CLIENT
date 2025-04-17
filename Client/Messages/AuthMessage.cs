using Client.Enums;

namespace Client.Messages;

public class AuthMessage(string username, string displayName, string secret, ushort messageId = 0) : IMessage
{
    public MessageType Type => MessageType.Auth;
    public ushort MessageId { get; } = messageId;
    public string Username { get; } = username;
    public string DisplayName { get; } = displayName;
    public string Secret { get; } = secret;
}