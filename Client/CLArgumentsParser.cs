// Martin Knor [xknorm01]
// FIT VUT
// 2025
namespace Client;

using CommandLine;
using Client.Enums;

public static class ClArgumentsParser
{
    public static ClOptions Parse(string[] args)
    {
        var options = new ClOptions();

        Parser.Default.ParseArguments<ClOptions>(args)
            .WithParsed(opts => options = opts)
            .WithNotParsed(HandleParseError);
        
        return options;
    }

    private static void HandleParseError(IEnumerable<Error> errs)
    {
        foreach (var err in errs)
        {
            if (err.Tag is ErrorType.HelpRequestedError or ErrorType.VersionRequestedError)
            {
                ExitHandler.Success();
            }
        }
        ExitHandler.Error(ExitCode.CommandLineError);
    }
}

public record ClOptions
{
    [Option('t', Required = true,
        HelpText = "Transport protocol type [tcp|udp].")]
    public string ProtocolType { get; set; } = string.Empty;

    [Option('s', Required = true, 
        HelpText = "Domain name or IPv4 address of the server to connect to.")]
    public string ServerString { get; set; } = string.Empty;

    [Option('p', Default = 4567, 
            HelpText = "Server port")]
    public int Port { get; set; }

    [Option('d', Default = 250, 
            HelpText = "UDP confirmation timeout (in milliseconds).")]
    public int UdpTimeout { get; set; }

    [Option('r', Default = 3, 
        HelpText = "Maximum number of UDP retransmissions.")]
    public int Retransmissions { get; set; }    
    
    [Option("discord", Default = false, 
        HelpText = "Enable connection via Discord integration.")]
    public bool Discord { get; set; }
}