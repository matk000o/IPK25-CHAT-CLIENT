using Client.Enums;
using Client.Messages;

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
        await client.SendMessageAsync(MessageFactory.BuildJoinMessage(channelId, client.DisplayName, client.Discord));
    }
}