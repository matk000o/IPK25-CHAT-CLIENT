namespace Client.Commands;

public class RenameCommand : ICommand
{
    public Task ExecuteAsync(TcpChatClient client, string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("ERROR: /rename requires a new display name");
            return Task.CompletedTask;
        }
        client.DisplayName = args[1];
        Console.WriteLine($"Display name changed to {client.DisplayName}");
        return Task.CompletedTask;
    }
}