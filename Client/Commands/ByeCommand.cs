namespace Client.Commands;

public class ByeCommand : ICommand
{
    public async Task ExecuteAsync(TcpChatClient client, string[] args)
    {
        await client.ShutdownAsync();
    }
}