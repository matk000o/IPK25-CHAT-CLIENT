using Client.Enums;
using Client.Messages;

namespace Client.Commands;

public class JoinCommand : Command
{
    public override async Task ExecuteAsync(IChatClient client, string command)
    {
        var args = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
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
        client.State = ClientState.Join;
        byte[] message = MessageBuilder.BuildJoinMessage(channelId, client.DisplayName, client.Discord);
        await client.SendMessageAsync(message);
    }
}