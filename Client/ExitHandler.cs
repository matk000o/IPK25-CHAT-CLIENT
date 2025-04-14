using CommandLine;

namespace Client;

using System;
using Client.Enums;
public abstract class ExitHandler
{
    public static void Success()
    {
        Environment.Exit((int)ExitCode.Success);
    }

    public static void Error(ExitCode exitCode, string? message = null)
    {
        if (message != null)
            Console.Error.WriteLine($"ERROR: {message}");
        Environment.Exit((int)exitCode);
    }
}   