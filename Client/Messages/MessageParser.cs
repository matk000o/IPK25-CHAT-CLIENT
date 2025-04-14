using System.Text.RegularExpressions;
using Client.Constants;

namespace Client.Messages;

public static class MessageParser
{
    public static IMessage Parse(string rawMessage)
    {
        if (rawMessage.StartsWith("REPLY", StringComparison.OrdinalIgnoreCase))
        {
            // Pattern: REPLY SP (OK|NOK) SP IS SP {content}
            const string pattern = $@"^REPLY (OK|NOK) IS ({RegexPatterns.ContentPattern})$";
            Regex replyRegex = new(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var match = replyRegex.Match(rawMessage);
            if (match.Success)
            {
                bool success = match.Groups[1].Value.Equals("OK", StringComparison.OrdinalIgnoreCase);
                string content = match.Groups[2].Value;
                return new ReplyMessage(success, content, rawMessage);
            }
        }
        else if (rawMessage.StartsWith("MSG", StringComparison.OrdinalIgnoreCase))
        {
            // Pattern: MSG SP FROM SP {DNAME} SP IS SP {CONTENT}
            const string pattern = $@"^MSG FROM ({RegexPatterns.DNamePattern}) IS ({RegexPatterns.ContentPattern})$";
            Regex msgRegex = new(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var match = msgRegex.Match(rawMessage);
            if (match.Success)
            {
                string sender = match.Groups[1].Value;
                string message = match.Groups[2].Value;
                return new ChatMessage(sender, message, rawMessage);
            }
        }
        else if (rawMessage.StartsWith("ERR", StringComparison.OrdinalIgnoreCase))
        {
            // Pattern: ERR SP FROM SP {DNAME} SP IS SP {CONTENT}
            const string pattern = $@"^ERR FROM ({RegexPatterns.DNamePattern}) IS ({RegexPatterns.ContentPattern})$";
            Regex errRegex = new(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var match = errRegex.Match(rawMessage);
            if (match.Success)
            {
                string sender = match.Groups[1].Value;
                string error = match.Groups[2].Value;
                return new ErrorMessage(sender, error, rawMessage);
            }
        }
        else if (rawMessage.StartsWith("BYE", StringComparison.OrdinalIgnoreCase))
        {
            // Pattern: BYE SP FROM SP {DNAME}
            const string pattern = $@"^BYE FROM ({RegexPatterns.DNamePattern})$";
            Regex byeRegex = new(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var match = byeRegex.Match(rawMessage);
            if (match.Success)
            {
                string sender = match.Groups[1].Value;
                return new ByeMessage(sender, rawMessage);
            }
        }
        
        // If nothing matches, return an unknown message.
        return new UnknownMessage(rawMessage);
    }
}