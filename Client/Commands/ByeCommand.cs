namespace Client.Commands;

public class ByeCommand : Command
{
    public override async Task ExecuteAsync(IChatClient client, string command)
    {
        await client.ShutdownAsync();
    }
}