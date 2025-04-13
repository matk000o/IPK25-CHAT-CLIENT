using System.Text.RegularExpressions;
using Client.Enums;

namespace Client.Messages;

public static class MessageParser
{
    public static IMessage Parse(string rawMessage)
    {
        if (rawMessage.StartsWith("REPLY"))
        {
            // Pattern: REPLY (OK|NOK) IS {content}
            Regex replyPattern = new(@"^REPLY\s(OK|NOK)\sIS\s(.+)$", RegexOptions.Compiled);
            var match = replyPattern.Match(rawMessage);
            if (match.Success)
            {
                bool success = match.Groups[1].Value == "OK";
                string content = match.Groups[2].Value;
                return new ReplyMessage(success, content, rawMessage);
            }
        }
        else if (rawMessage.StartsWith("MSG"))
        {
            // Pattern: MSG FROM {sender} IS {message}
            Regex chatPattern = new(@"^MSG\sFROM\s(\S+)\sIS\s(.+)$", RegexOptions.Compiled);
            var match = chatPattern.Match(rawMessage);
            if (match.Success)
            {
                string sender = match.Groups[1].Value;
                string message = match.Groups[2].Value;
                return new ChatMessage(sender, message, rawMessage);
            }
        }
        else if (rawMessage.StartsWith("ERR"))
        {
            // Pattern: ERR FROM {sender} IS {error}
            Regex errPattern = new(@"^ERR\sFROM\s(\S+)\sIS\s(.+)$", RegexOptions.Compiled);
            var match = errPattern.Match(rawMessage);
            if (match.Success)
            {
                string sender = match.Groups[1].Value;
                string error = match.Groups[2].Value;
                return new ErrorMessage(sender, error, rawMessage);
            }
        }
        else if (rawMessage.StartsWith("BYE"))
        {
            // Pattern: BYE FROM {sender}
            Regex byePattern = new(@"^BYE\sFROM\s(\S+)$", RegexOptions.Compiled);
            var match = byePattern.Match(rawMessage);
            if (match.Success)
            {
                string sender = match.Groups[1].Value;
                return new ByeMessage(sender, rawMessage);
            }
        }
        
        // If nothing matches, return an "unknown" message.
        return new UnknownMessage(rawMessage);
    }
}

public class UnknownMessage : IMessage
{
    public MessageType Type => MessageType.Unknown;
    public string RawText { get; }
    public UnknownMessage(string rawText)
    {
        RawText = rawText;
    }
}

