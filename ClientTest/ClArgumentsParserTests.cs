using Client;

namespace ClientTest;

public class ClArgumentsParserTests
{
    [Theory]
    [InlineData("-t tcp -s localhost",
        "tcp", "localhost",4567, 250, 3)]
    [InlineData("-t tcp -s anton5.fit.vutbr.cz",
        "tcp", "anton5.fit.vutbr.cz",4567, 250, 3)]
    [InlineData("-t udp -s 127.0.0.1 -p 4321 -d 500 -r 5",
        "udp", "127.0.0.1",4321, 500, 5)]
    public void Parse_ValidValues(
        string argLine,string protocolType, string hostname, int port, int timeout, int retries)
    {
        var args = argLine.Split(' ');
        var opts = ClArgumentsParser.Parse(args);

        Assert.Equal(protocolType, opts.ProtocolType);
        Assert.Equal(hostname, opts.ServerString);
        Assert.Equal(port, opts.Port);
        Assert.Equal(timeout, opts.UdpTimeout);
        Assert.Equal(retries, opts.Retransmissions);
    }
}