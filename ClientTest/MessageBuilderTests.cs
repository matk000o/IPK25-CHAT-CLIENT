using Xunit;
using Client.Messages;
using Client.Enums;
using System.Text;

namespace ClientTest;

public class MessageBuilderTests
{
    [Fact]
    public void BuildAuthMessage_Tcp_FormatCorrect()
    {
        MessageBuilder.CurrentClientProtocol = ClientProtocolType.Tcp;
        var payload = MessageBuilder.BuildAuthMessage("user1", "Alice", "secret");
        var text = Encoding.ASCII.GetString(payload);
        Assert.Equal("AUTH user1 AS Alice USING secret\r\n", text);
    }

    [Fact]
    public void BuildAuthMessage_Udp_HeaderAndFieldsCorrect()
    {
        MessageBuilder.CurrentClientProtocol = ClientProtocolType.Udp;
        var payload = MessageBuilder.BuildAuthMessage("user1", "Alice", "secret");
        // first byte is message type
        Assert.Equal((byte)MessageType.Auth, payload[0]);
        // next two bytes are the auto-generated messageId
        // body should be "user1\0Alice\0secret\0"
        var body = Encoding.ASCII.GetString(payload, 3, payload.Length - 3);
        Assert.Equal("user1\0Alice\0secret\0", body);
    }

    [Fact]
    public void BuildConfirmMessage_CorrectBytes()
    {
        ushort messageId = 0x1234;
        var msg = MessageBuilder.BuildConfirmMessage(messageId);
        Assert.Equal(3, msg.Length);
        Assert.Equal((byte)MessageType.Confirm, msg[0]);
        Assert.Equal((byte)(messageId >> 8), msg[1]);
        Assert.Equal((byte)(messageId & 0xFF), msg[2]);
    }

    [Fact]
    public void BuildJoinMessage_Tcp_FormatCorrect()
    {
        MessageBuilder.CurrentClientProtocol = ClientProtocolType.Tcp;
        var payload = MessageBuilder.BuildJoinMessage("chan1", "Bob", false);
        var text = Encoding.ASCII.GetString(payload);
        Assert.Equal("JOIN chan1 AS Bob\r\n", text);
    }

    [Fact]
    public void BuildJoinMessage_Udp_HeaderAndPayloadCorrect()
    {
        MessageBuilder.CurrentClientProtocol = ClientProtocolType.Udp;
        var payload = MessageBuilder.BuildJoinMessage("chan1", "Bob", false);
        Assert.Equal((byte)MessageType.Join, payload[0]);
        var body = Encoding.ASCII.GetString(payload, 3, payload.Length - 3);
        Assert.Equal("chan1\0Bob\0", body);
    }

    [Fact]
    public void BuildChatMessage_Tcp_FormatCorrect()
    {
        MessageBuilder.CurrentClientProtocol = ClientProtocolType.Tcp;
        var payload = MessageBuilder.BuildChatMessage("Bob", "Hello!");
        var text = Encoding.ASCII.GetString(payload);
        Assert.Equal("MSG FROM Bob IS Hello!\r\n", text);
    }

    [Fact]
    public void BuildChatMessage_Udp_HeaderAndPayloadCorrect()
    {
        MessageBuilder.CurrentClientProtocol = ClientProtocolType.Udp;
        var payload = MessageBuilder.BuildChatMessage("Bob", "Hello!");
        Assert.Equal((byte)MessageType.Msg, payload[0]);
        var body = Encoding.ASCII.GetString(payload, 3, payload.Length - 3);
        Assert.Equal("Bob\0Hello!\0", body);
    }

    [Fact]
    public void BuildByeMessage_Tcp_FormatCorrect()
    {
        MessageBuilder.CurrentClientProtocol = ClientProtocolType.Tcp;
        var payload = MessageBuilder.BuildByeMessage("Carol");
        var text = Encoding.ASCII.GetString(payload);
        Assert.Equal("BYE FROM Carol\r\n", text);
    }

    [Fact]
    public void BuildByeMessage_Udp_HeaderAndPayloadCorrect()
    {
        MessageBuilder.CurrentClientProtocol = ClientProtocolType.Udp;
        var payload = MessageBuilder.BuildByeMessage("Carol");
        Assert.Equal((byte)MessageType.Bye, payload[0]);
        var body = Encoding.ASCII.GetString(payload, 3, payload.Length - 3);
        Assert.Equal("Carol\0", body);
    }

    [Fact]
    public void BuildErrorMessage_Tcp_FormatCorrect()
    {
        MessageBuilder.CurrentClientProtocol = ClientProtocolType.Tcp;
        var payload = MessageBuilder.BuildErrorMessage("Dave", "Oops");
        var text = Encoding.ASCII.GetString(payload);
        Assert.Equal("ERR FROM Dave IS Oops\r\n", text);
    }

    [Fact]
    public void BuildErrorMessage_Udp_HeaderAndPayloadCorrect()
    {
        MessageBuilder.CurrentClientProtocol = ClientProtocolType.Udp;
        var payload = MessageBuilder.BuildErrorMessage("Dave", "Oops");
        Assert.Equal((byte)MessageType.Err, payload[0]);
        var body = Encoding.ASCII.GetString(payload, 3, payload.Length - 3);
        Assert.Equal("Dave\0Oops\0", body);
    }
}
