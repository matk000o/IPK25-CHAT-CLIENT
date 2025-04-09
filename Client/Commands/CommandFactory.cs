namespace Client.Commands;

public static class CommandFactory
{
    private static readonly Dictionary<string, Func<IClientCommand>> RegisteredCommands = new()
    {
        { "/auth", () => new AuthCommand() },
        { "/join", () => new JoinCommand() },
        { "/rename", () => new RenameCommand() },
        { "/help", () => new HelpCommand() },
    };

    public static IClientCommand ParseInput(string input) 
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input is empty.");

        var tokens = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        var commandKey = tokens[0].ToLowerInvariant();

        if (RegisteredCommands.TryGetValue(commandKey, out var factory))
        {
            var command = factory();
            command.Parse(tokens.Skip(1).ToArray());
            return command;
        }

        var message = new MessageCommand();
        message.Parse([input]);
        return message;
    }
}