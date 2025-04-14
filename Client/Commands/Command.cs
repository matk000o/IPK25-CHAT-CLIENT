using System.Text.RegularExpressions;
using Client.Constants;

namespace Client.Commands;

public abstract class Command
{
    public abstract Task ExecuteAsync(TcpChatClient client, string[] args);

    protected static bool CheckId(string arg)
    {
        if (Regex.IsMatch(arg, RegexPatterns.AnchoredIdPattern)) return true;
        Console.WriteLine("ERROR: ID = 1*20 ( ALPHA / DIGIT / \"_\" / \"-\" )");
        return false;
    }
    protected static bool CheckSecret(string arg)
    {
        if (Regex.IsMatch(arg, RegexPatterns.AnchoredSecretPattern)) return true;
        Console.WriteLine("ERROR: <secret> = 1*128 ( ALPHA / DIGIT / \"_\" / \"-\" )");
        return false;
    }
    protected static bool CheckContent(string arg)
    {
        if (Regex.IsMatch(arg, RegexPatterns.AnchoredContentPattern)) return true;
        Console.WriteLine("ERROR: <content> = 1*60000 ( VCHAR / SP / LF )");
        return false;
    }
    protected static bool CheckDisplayName(string arg)
    {
        if (Regex.IsMatch(arg, RegexPatterns.AnchoredDNamePattern)) return true;
        Console.WriteLine("ERROR: <displayName> = 1*20 VCHAR");
        return false;
    }

    protected static void PrintHelp()
    {
        Console.WriteLine("\nSupported commands:");
        Console.WriteLine("/auth <ID(username)> <secret> <displayName> - Authenticate to the server.");
        Console.WriteLine("/join <ID(chanel)>                          - Join a channel.");
        Console.WriteLine("/rename <displayName>                       - Change your display name.");
        Console.WriteLine("/bye                                        - Disconnect from the server(same as ^c or ^d).");
        Console.WriteLine("/help                                       - Show this help message.");
        Console.WriteLine("any other input that doesn't start with '/' is interpreted as chat message.\n");
    }
}