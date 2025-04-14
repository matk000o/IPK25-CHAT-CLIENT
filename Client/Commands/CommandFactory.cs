namespace Client.Commands;

public static class CommandFactory
{
    public static Command? GetCommand(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
            return new ChatCommand();
        var parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string commandName = parts[0].ToLower();
        if (!commandName.StartsWith('/'))
            return new ChatCommand();
        
        return commandName switch
        {
            "/auth"  => new AuthCommand(),
            "/join"  => new JoinCommand(),
            "/bye"   => new ByeCommand(),
            "/help"  => new HelpCommand(),
            "/rename" => new RenameCommand(),
            
            // Add other commands as needed.
            _ => null,
        };
    }
}