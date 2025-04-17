namespace Client.Commands;

public class HelpCommand : Command
{
    public override Task ExecuteAsync(IChatClient client, string command)
    {
        PrintHelp();
        return Task.CompletedTask;
    } 
}