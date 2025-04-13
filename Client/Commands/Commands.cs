namespace Client.Commands;

public interface ICommand
{
    Task ExecuteAsync(TcpChatClient client, string[] args);
}

public class AuthCommand : ICommand
{
    public async Task ExecuteAsync(TcpChatClient client, string[] args)
    {
        if (args.Length < 4)
        {
            Console.WriteLine("ERROR: usage: /auth {username} {secret} {displayName}");
            return;
        }
        string username = args[1];
        string secret = args[2];
        string displayName = args[3];

        client.DisplayName = displayName;
        client.State = Client.Enums.ClientState.Auth;
        string authMsg = $"AUTH {username} AS {displayName} USING {secret}";
        await client.SendMessageAsync(authMsg);
    }
}

public class JoinCommand : ICommand
{
    public async Task ExecuteAsync(TcpChatClient client, string[] args)
    {
        if (client.State != Client.Enums.ClientState.Open)
        {
            Console.WriteLine("ERROR: You must be in the Open state to join a channel.");
            return;
        }
        if (args.Length < 2)
        {
            Console.WriteLine("ERROR: usage: /join {channelId}");
            return;
        }
        string channelId = args[1];
        client.State = Client.Enums.ClientState.Join;
        string joinMsg = $"JOIN {channelId} AS {client.DisplayName}";
        await client.SendMessageAsync(joinMsg);
    }
}

public class ByeCommand : ICommand
{
    public async Task ExecuteAsync(TcpChatClient client, string[] args)
    {
        await client.ShutdownAsync();
    }
}

// Define additional commands for /rename, /help etc.

public static class CommandFactory
{
    public static ICommand? GetCommand(string commandName)
    {
        return commandName switch
        {
            "/auth" => new AuthCommand(),
            "/join" => new JoinCommand(),
            "/bye"  => new ByeCommand(),
            // Add other commands as needed.
            _ => null,
        };
    }
}

