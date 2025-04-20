using System.Text;
using Client.Enums;

namespace Client.Messages;

public static class MessageBuilder
{
    // Select which protocol style to use.
    public static ClientProtocolType CurrentClientProtocol { get; set; }

    // Used for UDP messages – each message gets a unique 16-bit ID.
    private static ushort _udpMessageIdCounter;
    private static ushort GetNextUdpMessageId() => _udpMessageIdCounter++;

    // TCP messages: simply a text message terminated with CRLF.
    private static byte[] BuildTcpMessage(string messageText) =>
        Encoding.ASCII.GetBytes(messageText + "\r\n");

    // UDP messages: build a 3-byte header (Type, MessageID) then all fields as ASCII strings terminated by null.
    private static byte[] BuildUdpMessage(MessageType messageType, params string[] fields)
    {
        ushort msgId = GetNextUdpMessageId();
        byte[] header = new byte[3];
        header[0] = (byte)messageType;
        header[1] = (byte)(msgId >> 8);     // High byte
        header[2] = (byte)(msgId & 0xFF);     // Low byte

        var body = new List<byte>();
        foreach (string field in fields)
        {
            body.AddRange(Encoding.ASCII.GetBytes(field));
            body.Add(0); // Append null terminator to each field.
        }

        return header.Concat(body).ToArray();
    }
    
    public static byte[] BuildConfirmMessage(ushort messageId)
    {
        return
        [
            (byte)MessageType.Confirm,
            (byte)(messageId >> 8),
            (byte)(messageId & 0xFF)
        ];
    }

    /// <summary>
    /// Builds the AUTH message.
    /// - TCP: "AUTH {username} AS {displayName} USING {secret}\r\n"
    /// - UDP: [UdpMessageType.Auth][MessageID][username][0][displayName][0][secret][0]
    /// </summary>
    public static byte[] BuildAuthMessage(string username, string displayName, string secret)
    {
        return CurrentClientProtocol == ClientProtocolType.Tcp 
            ? BuildTcpMessage($"AUTH {username} AS {displayName} USING {secret}") 
            : BuildUdpMessage(MessageType.Auth, username, displayName, secret);
    }

    /// <summary>
    /// Builds the JOIN message.
    /// - TCP: "JOIN {channelId} AS {displayName}\r\n"
    /// - UDP: [UdpMessageType.Join][MessageID][channelId][0][displayName][0]
    /// For Discord, channelId gets prefixed with "discord.".
    /// </summary>
    public static byte[] BuildJoinMessage(string channelId, string displayName, bool useDiscord)
    {
        if (useDiscord)
            channelId = "discord." + channelId;
        
        return CurrentClientProtocol == ClientProtocolType.Tcp 
            ? BuildTcpMessage($"JOIN {channelId} AS {displayName}") 
            : BuildUdpMessage(MessageType.Join, channelId, displayName);
    }

    /// <summary>
    /// Builds the chat message.
    /// - TCP: "MSG FROM {displayName} IS {content}\r\n"
    /// - UDP: [UdpMessageType.Msg][MessageID][displayName][0][content][0]
    /// </summary>
    public static byte[] BuildChatMessage(string displayName, string content)
    {
        return CurrentClientProtocol == ClientProtocolType.Tcp 
            ? BuildTcpMessage($"MSG FROM {displayName} IS {content}") 
            : BuildUdpMessage(MessageType.Msg, displayName, content);
    }

    /// <summary>
    /// Builds the BYE message.
    /// - TCP: "BYE FROM {displayName}\r\n"
    /// - UDP: [UdpMessageType.Bye][MessageID][displayName][0]
    /// </summary>
    public static byte[] BuildByeMessage(string displayName)
    {
        return CurrentClientProtocol == ClientProtocolType.Tcp 
            ? BuildTcpMessage($"BYE FROM {displayName}") 
            : BuildUdpMessage(MessageType.Bye, displayName);
    }

    /// <summary>
    /// Builds the error message.
    /// - TCP: "ERR FROM {displayName} IS {error}\r\n"
    /// - UDP: [UdpMessageType.Err][MessageID][displayName][0][error][0]
    /// </summary>
    public static byte[] BuildErrorMessage(string displayName, string error)
    {
        return CurrentClientProtocol == ClientProtocolType.Tcp 
            ? BuildTcpMessage($"ERR FROM {displayName} IS {error}") 
            : BuildUdpMessage(MessageType.Err, displayName, error);
    }
}