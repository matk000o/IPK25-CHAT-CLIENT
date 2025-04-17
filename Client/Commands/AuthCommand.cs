using Client.Enums;
using Client.Messages;

namespace Client.Commands;

public class AuthCommand : Command
{
    public override async Task ExecuteAsync(IChatClient client, string command)
    {
        if (client.State != ClientState.Auth && client.State != ClientState.Start)
        {
            Console.WriteLine("ERROR: you are already authorized");
            return ;
        }
        var args = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (args.Length != 4)
        {
            Console.WriteLine("ERROR: usage: /auth {username} {secret} {displayName}");
            return;
        }
        
        string username = args[1];
        string secret = args[2];
        string displayName = args[3];
        if (!CheckId(username) || !CheckSecret(secret) || !CheckDisplayName(displayName))
            return;
        
        client.DisplayName = displayName;
        client.State = ClientState.Auth;
        byte[] message = MessageBuilder.BuildAuthMessage(username, displayName, secret);
        await client.SendMessageAsync(message);
    }
}