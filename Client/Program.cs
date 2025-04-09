// Martin Knor [xknorm01]
// FIT VUT
// 2025

using Client;
using Client.Tcp;
using Client.Enums;

class Program
{
    static async Task Main(string[] args)
    {
        var options = ClArgumentsParser.Parse(args);

        if (options.ProtocolType.ToLower() == "tcp")
        {
            var tcpClient = new TcpChatClient(options.ServerString, options.Port);
            await tcpClient.RunAsync();
        }
        else if (options.ProtocolType.ToLower() == "udp")
        {
            Console.WriteLine("UDP is not implemented yet.");
            ExitHandler.Error(ExitCode.UnsupportedProtocol);
        }
        else
        {
            Console.WriteLine("Invalid protocol type. Use 'tcp' or 'udp'.");
            ExitHandler.Error(ExitCode.CommandLineError);
        }
    }
}
