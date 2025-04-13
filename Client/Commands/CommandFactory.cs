namespace Client.Commands;

public static class CommandFactory
{
    public static ICommand? GetCommand(string commandName)
    {
        return commandName switch
        {
            "/auth" => new AuthCommand(),
            "/join" => new JoinCommand(),
            "/bye"  => new ByeCommand(),
            // Add other commands as needed.
            _ => null,
        };
    }
}