namespace Client.Commands;

public class HelpCommand : Command
{
    public override Task ExecuteAsync(TcpChatClient client, string[] args)
    {
        PrintHelp();
        return Task.CompletedTask;
    } 
}