using Client.Messages;
using Client.Enums;
using System.Text;

namespace ClientTest;

public class MessageParserTests
{
    // --- TCP parsing tests ---

    [Theory]
    [InlineData("REPLY OK IS Welcome", true, "Welcome")]
    [InlineData("REPLY NOK IS ErrorOccurred", false, "ErrorOccurred")]
    public void ParseTcp_ReplyMessages(string raw, bool expectedSuccess, string expectedContent)
    {
        var msg = MessageParser.ParseTcp(raw) as ReplyMessage;
        Assert.NotNull(msg);
        Assert.Equal(expectedSuccess, msg.Success);
        Assert.Equal(expectedContent, msg.Content);
    }

    [Fact]
    public void ParseTcp_ChatMessage()
    {
        var raw = "MSG FROM Alice IS HelloWorld";
        var msg = MessageParser.ParseTcp(raw) as ChatMessage;
        Assert.NotNull(msg);
        Assert.Equal("Alice", msg.Sender);
        Assert.Equal("HelloWorld", msg.MessageContent);
    }

    [Fact]
    public void ParseTcp_ErrorMessage()
    {
        var raw = "ERR FROM Bob IS SomethingWentWrong";
        var msg = MessageParser.ParseTcp(raw) as ErrorMessage;
        Assert.NotNull(msg);
        Assert.Equal("Bob", msg.Sender);
        Assert.Equal("SomethingWentWrong", msg.ErrorContent);
    }

    [Fact]
    public void ParseTcp_ByeMessage()
    {
        var raw = "BYE FROM Carol";
        var msg = MessageParser.ParseTcp(raw) as ByeMessage;
        Assert.NotNull(msg);
        Assert.Equal("Carol", msg.Sender);
    }

    [Fact]
    public void ParseTcp_Unknown()
    {
        var msg = MessageParser.ParseTcp("GARBAGE INPUT");
        Assert.IsType<UnknownMessage>(msg);
    }

    // --- UDP parsing tests ---

    [Fact]
    public void ParseUdp_ConfirmMessage()
    {
        ushort id = 0x1234;
        var data = new[] { (byte)MessageType.Confirm, (byte)(id >> 8), (byte)(id & 0xFF) };
        var msg = MessageParser.ParseUdp(data) as ConfirmMessage;
        Assert.NotNull(msg);
        Assert.Equal(id, msg.MessageId);
    }

    [Fact]
    public void ParseUdp_PingMessage()
    {
        ushort id = 0x00FF;
        var data = new[] { (byte)MessageType.Ping, (byte)(id >> 8), (byte)(id & 0xFF) };
        var msg = MessageParser.ParseUdp(data) as PingMessage;
        Assert.NotNull(msg);
        Assert.Equal(id, msg.MessageId);
    }

    [Fact]
    public void ParseUdp_ReplyMessage()
    {
        ushort msgId = 0x0203;
        ushort refId = 0x0102;
        var content = "ServerOK";
        var header = new[] {
            (byte)MessageType.Reply,
            (byte)(msgId >> 8), (byte)(msgId & 0xFF)
        };
        var payload = new byte[] {
            1,                                     // success = true
            (byte)(refId >> 8), (byte)(refId & 0xFF)
        };
        var bodyBytes = Encoding.ASCII.GetBytes(content).Concat(new byte[]{0}).ToArray();
        var data = header.Concat(payload).Concat(bodyBytes).ToArray();

        var msg = MessageParser.ParseUdp(data) as ReplyMessage;
        Assert.NotNull(msg);
        Assert.Equal(msgId, msg.MessageId);
        Assert.Equal(refId, msg.ReferenceMessageId);
        Assert.True(msg.Success);
        Assert.Equal(content, msg.Content);
    }

    [Fact]
    public void ParseUdp_ChatMessage()
    {
        ushort msgId = 0x0A0B;
        var sender = "Dave";
        var text = "HiThere";
        var header = new[] {
            (byte)MessageType.Msg,
            (byte)(msgId >> 8), (byte)(msgId & 0xFF)
        };
        var body = Encoding.ASCII.GetBytes($"{sender}\0{text}\0");
        var data = header.Concat(body).ToArray();

        var msg = MessageParser.ParseUdp(data) as ChatMessage;
        Assert.NotNull(msg);
        Assert.Equal(msgId, msg.MessageId);
        Assert.Equal(sender, msg.Sender);
        Assert.Equal(text, msg.MessageContent);
    }

    [Fact]
    public void ParseUdp_ErrorMessage()
    {
        ushort msgId = 0x0C0D;
        var sender = "Eve";
        var error = "BadStuff";
        var header = new[] {
            (byte)MessageType.Err,
            (byte)(msgId >> 8), (byte)(msgId & 0xFF)
        };
        var body = Encoding.ASCII.GetBytes($"{sender}\0{error}\0");
        var data = header.Concat(body).ToArray();

        var msg = MessageParser.ParseUdp(data) as ErrorMessage;
        Assert.NotNull(msg);
        Assert.Equal(msgId, msg.MessageId);
        Assert.Equal(sender, msg.Sender);
        Assert.Equal(error, msg.ErrorContent);
    }

    [Fact]
    public void ParseUdp_ByeMessage()
    {
        ushort msgId = 0x0E0F;
        var sender = "Frank";
        var header = new[] {
            (byte)MessageType.Bye,
            (byte)(msgId >> 8), (byte)(msgId & 0xFF)
        };
        var body = Encoding.ASCII.GetBytes($"{sender}\0");
        var data = header.Concat(body).ToArray();

        var msg = MessageParser.ParseUdp(data) as ByeMessage;
        Assert.NotNull(msg);
        Assert.Equal(msgId, msg.MessageId);
        Assert.Equal(sender, msg.Sender);
    }

    [Fact]
    public void ParseUdp_Malformed_ReturnsUnknown()
    {
        // too short to be valid (less than 3-byte header)
        var data = new byte[] { 0x01, 0x02 };
        var msg = MessageParser.ParseUdp(data);
        Assert.IsType<UnknownMessage>(msg);
    }
}

