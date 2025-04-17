using Client.Enums;

namespace Client.Messages;

public class JoinMessage(string channelId, string displayName, ushort messageId = 0) : IMessage
{
    public MessageType Type => MessageType.Join;
    public ushort MessageId { get; } = messageId;
    public string ChannelId { get; } = channelId;
    public string DisplayName { get; } = displayName;
}