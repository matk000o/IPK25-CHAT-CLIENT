using Client.Enums;

namespace Client.Messages;

public interface IMessage
{
    MessageType Type { get; }
    string RawText { get; }
}

public class ReplyMessage : IMessage
{
    public MessageType Type => MessageType.Reply;
    public string RawText { get; }
    public bool Success { get; }
    public string Content { get; }
    
    public ReplyMessage(bool success, string content, string rawText)
    {
        Success = success;
        Content = content;
        RawText = rawText;
    }
}

public class ChatMessage : IMessage
{
    public MessageType Type => MessageType.Chat;
    public string RawText { get; }
    public string Sender { get; }
    public string MessageContent { get; }
    
    public ChatMessage(string sender, string messageContent, string rawText)
    {
        Sender = sender;
        MessageContent = messageContent;
        RawText = rawText;
    }
}

public class ErrorMessage : IMessage
{
    public MessageType Type => MessageType.Error;
    public string RawText { get; }
    public string Sender { get; }
    public string ErrorContent { get; }
    
    public ErrorMessage(string sender, string errorContent, string rawText)
    {
        Sender = sender;
        ErrorContent = errorContent;
        RawText = rawText;
    }
}

public class ByeMessage : IMessage
{
    public MessageType Type => MessageType.Bye;
    public string RawText { get; }
    public string Sender { get; }
    
    public ByeMessage(string sender, string rawText)
    {
        Sender = sender;
        RawText = rawText;
    }
}
