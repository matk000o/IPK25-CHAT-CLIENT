using Client.Enums;

namespace Client.Commands;

public class JoinCommand : Command
{
    public override async Task ExecuteAsync(TcpChatClient client, string[] args)
    {
        if (client.State != ClientState.Open)
        {
            Console.WriteLine("ERROR: You must be in the Open state to join a channel.");
            return;
        }
        if (args.Length != 2)
        {
            Console.WriteLine("ERROR: usage: /join {channelId}");
            return;
        }
        string channelId = args[1];
        if (!CheckId(channelId))
            return;
        if (client.Discord)
            channelId = $"discord.{channelId}";
        client.State = ClientState.Join;
        string joinMsg = $"JOIN {channelId} AS {client.DisplayName}";
        await client.SendMessageAsync(joinMsg);
    }
}