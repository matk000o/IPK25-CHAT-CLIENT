namespace Client.Commands;

public class ByeCommand : Command
{
    public override async Task ExecuteAsync(TcpChatClient client, string command)
    {
        await client.ShutdownAsync();
    }
}