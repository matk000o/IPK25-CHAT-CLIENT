using Client.Enums;
using Client.Messages;

namespace Client.Commands;

public class AuthCommand : Command
{
    public override async Task ExecuteAsync(TcpChatClient client, string command)
    {
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
        await client.SendMessageAsync(MessageFactory.BuildAuthMessage(username, displayName, secret));
    }
}