namespace Client.Commands;

public class HelpCommand : ICommand
{
    public Task ExecuteAsync(TcpChatClient client, string[] args)
    {
        Console.WriteLine("Supported commands:");
        Console.WriteLine("/auth <username> <secret> <displayName> - Authenticate to the server.");
        Console.WriteLine("/join <channelId> - Join a channel.");
        Console.WriteLine("/rename <displayName> - Change your display name.");
        Console.WriteLine("/help - Show this help message.");
        Console.WriteLine("/bye - Disconnect from the server.");
        return Task.CompletedTask;
    } 
}