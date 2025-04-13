namespace Client;

static class Program
{
    static async Task Main(string[] args)
    {
        var options = ClArgumentsParser.Parse(args);

        if (options.ProtocolType.ToLower() == "tcp")
        {
            var tcpClient = new TcpChatClient(options.ServerString, options.Port);

            Console.CancelKeyPress += async (sender, e) =>
            {
                e.Cancel = true;
                Console.WriteLine("Interrupt detected. Disconnecting...");
                await tcpClient.ShutdownAsync();
            };
            
            await tcpClient.RunAsync();
        }
        else
        {
            throw new NotImplementedException();
        }
    }
}