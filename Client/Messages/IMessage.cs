using Client.Enums;

namespace Client.Messages;

public interface IMessage
{
    MessageType Type { get; }
    string RawText { get; }
}