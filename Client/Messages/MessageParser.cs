using System.Text;
using System.Text.RegularExpressions;
using Client.Constants;
using Client.Enums;

namespace Client.Messages;

public static class MessageParser
{
    public static IMessage ParseTcp(string rawMessage)
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
                return new ReplyMessage(success, content);
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
                return new ChatMessage(sender, message);
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
                return new ErrorMessage(sender, error);
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
                return new ByeMessage(sender);
            }
        }
        
        // If nothing matches, return an unknown message.
        return new UnknownMessage();
    }
    
    /// <summary>
    /// Parses a UDP message according to IPK25-CHAT spec.
    /// </summary>
    public static IMessage ParseUdp(byte[] udpMessage)
        {
            // Must have at least 3 bytes for header
            if (udpMessage.Length < 3)
                return new UnknownMessage();

            // 1-byte type, 2-byte MessageID (network order)
            var msgType = (MessageType)udpMessage[0];
            ushort msgId = (ushort)((udpMessage[1] << 8) | udpMessage[2]);

            // Extract payload bytes
            byte[] payload = udpMessage.Skip(3).ToArray();

            switch (msgType)
            {
                case MessageType.Confirm:
                    // No payload for Confirm
                    if (payload.Length == 0)
                        return new ConfirmMessage(msgId);
                    break;

                case MessageType.Reply:
                    // [0]=Result (0x00=NOK, 0x01=OK), [1-2]=RefMsgID, [3..]=Content\0
                    if (payload.Length >= 3)
                    {
                        bool success = payload[0] == 1;
                        ushort referenceMessageId = (ushort)((payload[1] << 8) | payload[2]);
                        string content = ReadNullTerminated(payload, 3);
                        return new ReplyMessage(success, content, msgId, referenceMessageId);
                    }
                    break;

                case MessageType.Msg:
                    // DisplayName\0MessageContent\0
                    var msgFields = SplitNullFields(payload);
                    if (msgFields.Length == 2)
                        return new ChatMessage(msgFields[0], msgFields[1], msgId);
                    break;

                case MessageType.Err:
                    // DisplayName\0ErrorContent\0
                    var errFields = SplitNullFields(payload);
                    if (errFields.Length == 2)
                        return new ErrorMessage(errFields[0], errFields[1], msgId);
                    break;

                case MessageType.Ping:
                    // No payload for Ping
                    if (payload.Length == 0)
                        return new PingMessage(msgId);
                    break;

                case MessageType.Bye:
                    // DisplayName\0
                    var byeFields = SplitNullFields(payload);
                    if (byeFields.Length == 1)
                        return new ByeMessage(byeFields[0], msgId);
                    break;

                // (Auth and Join are client->server only)
            }

            // Malformed or unsupported
            return new UnknownMessage(msgId);
        }

        /// <summary>
        /// Reads a null-terminated ASCII string starting at offset.
        /// </summary>
        private static string ReadNullTerminated(byte[] data, int offset)
        {
            int end = Array.IndexOf(data, (byte)0, offset);
            int length = (end >= 0 ? end : data.Length) - offset;
            return Encoding.ASCII.GetString(data, offset, length);
        }

        /// <summary>
        /// Splits payload into ASCII fields separated by 0 bytes.
        /// </summary>
        private static string[] SplitNullFields(byte[] data)
        {
            return Encoding.ASCII
                   .GetString(data)
                   .Split('\0', StringSplitOptions.RemoveEmptyEntries);
        }
}