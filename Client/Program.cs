using Client.Enums;
using Client.Messages;

namespace Client;

static class Program
{
    static async Task Main(string[] args)
    {
        var options = ClArgumentsParser.Parse(args);
        
        IChatClient? chatClient = null;

        if (options.ProtocolType.ToLower() == "tcp")
        {
            chatClient = new TcpChatClient(
                options.ServerString, options.Port, options.Discord);
        }
        else
        {
            chatClient = new UdpChatClient(
                options.ServerString, options.Port,options.UdpTimeout,options.Retransmissions, options.Discord);
            
        }
        
        Console.CancelKeyPress += async (sender, e) =>
        {
            e.Cancel = true;
            Console.WriteLine("Interrupt detected. Disconnecting...");
            await chatClient.ShutdownAsync();
        };
            
        await chatClient.RunAsync();
    }
}