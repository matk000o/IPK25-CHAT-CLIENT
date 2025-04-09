using System.Text.RegularExpressions;
using System.Windows.Input;
using Client.Enums;

namespace Client.Commands;

public abstract class CommandBase : IClientCommand
{
    public abstract CommandType Type { get; }
    public abstract void Parse(string[] args);

    // ID        = 1*20    ( ALPHA / DIGIT / "_" / "-" )
    public void CheckUsername(string username)
    {
        if (username.Length > 20)
            throw new Exception($"Username must be less than 20 characters.");
        if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_\-]+$"))
            throw new Exception($"Username doesn't match specified pattern.");
    }

    // SECRET    = 1*128   ( ALPHA / DIGIT / "_" / "-" )
    public void CheckSecret(string secret)
    {
        if (secret.Length > 128)
            throw new Exception($"Secret must be less than 128 characters.");
        if (!Regex.IsMatch(secret, @"^[a-zA-Z0-9_\-]+$"))
            throw new Exception($"Secret doesn't match specified pattern.");
    }

    // DNAME     = 1*20    VCHAR
    public void CheckDisplayName(string displayName)
    {
        if (displayName.Length > 20)
            throw new Exception($"Display name must be less than 20 characters.");
        //regex for VCHAR as is defined in https://www.rfc-editor.org/rfc/rfc5234
        if (!Regex.IsMatch(displayName, @"^[\x21-\x7e]+$")) 
            throw new Exception($"Display name doesn't match specified pattern.");
    }

    // ID        = 1*20    ( ALPHA / DIGIT / "_" / "-" )
    public void CheckChanelId(string chanelId)
    {
        if (chanelId.Length > 20)
            throw new Exception($"Chanel ID must be less than 20 characters.");
        if (!Regex.IsMatch(chanelId, @"^[a-zA-Z0-9_\-]+$"))
            throw new Exception($"Chanel ID doesn't match specified pattern.");
    }    
    
    // CONTENT   = 1*60000 ( VCHAR / SP / LF )
    public void CheckMessageContent(string message)
    {
        if (message.Length > 60000)
            throw new Exception($"Message must be less than 60000 characters.");
        if (!Regex.IsMatch(message, @"^[\x21-\x7e|\x20|\x0a]+$"))
            throw new Exception($"Message doesn't match specified pattern.");
    }
}

public class AuthCommand : CommandBase
{
    public override CommandType Type => CommandType.Auth;

    public string Username { get; private set; } = string.Empty;
    public string Secret { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;

    public override void Parse(string[] args)
    {
        if (args.Length != 3)
            throw new ArgumentException("/auth command expects 3 parameters\n   usage: {Username} {Secret} {DisplayName}");
        Username = args[0];
        CheckUsername(Username);
        Secret = args[1];
        CheckSecret(Secret);
        DisplayName = args[2];
        CheckDisplayName(DisplayName);
    }
}

public class JoinCommand : CommandBase
{
    public override CommandType Type => CommandType.Join;
    public string ChanelId { get; private set; } = string.Empty;
    
    public override void Parse(string[] args)
    {
        if (args.Length != 1)
            throw new ArgumentException("/join command expects 1 parameter\n   usage: /join {ChanelID}");
        ChanelId = args[0];
        CheckChanelId(ChanelId);
    }
}

public class RenameCommand : CommandBase
{
    public override CommandType Type => CommandType.Rename;
    public string NewDisplayName { get; private set; } = string.Empty;
    
    public override void Parse(string[] args)
    {
        if (args.Length != 1)
            throw new ArgumentException("/rename command expects 1 parameter\n   usage: /help {DisplayName}");
        NewDisplayName = args[0];
        CheckDisplayName(NewDisplayName);
    }
}

public class HelpCommand : CommandBase
{
    public override CommandType Type => CommandType.Help;
    public override void Parse(string[] args)
    {
        if (args.Length != 0)
            throw new ArgumentException("/help command expects 0 parameters\n   usage: /help");
    }
}

public class MessageCommand : CommandBase
{
    public override CommandType Type => CommandType.Message;
    
    public string MessageContent { get; private set; } = string.Empty;

    public override void Parse(string[] args)
    {
        if (args.Length != 1)
            throw new ArgumentException("message should be passed to for parsing as an array of one string");
        MessageContent = args[0];
    }
}

public class EmptyCommand : CommandBase
{
    public override CommandType Type => CommandType.Empty;
    public override void Parse(string[] args) { }
}