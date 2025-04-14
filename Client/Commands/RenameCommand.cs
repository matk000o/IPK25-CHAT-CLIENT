namespace Client.Commands;

public class RenameCommand : Command
{
    public override Task ExecuteAsync(TcpChatClient client, string command)
    {
        var args = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (args.Length != 2)
        {
            Console.WriteLine("ERROR: usage: /rename {DisplayName}");
            return Task.CompletedTask;
        }
        if (!CheckDisplayName(args[1]))
            return Task.CompletedTask;
        client.DisplayName = args[1];
        Console.WriteLine($"Display name changed to {client.DisplayName}");
        return Task.CompletedTask;
    }
}