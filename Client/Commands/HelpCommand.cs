namespace Client.Commands;

public class HelpCommand : Command
{
    public override Task ExecuteAsync(TcpChatClient client, string command)
    {
        PrintHelp();
        return Task.CompletedTask;
    } 
}