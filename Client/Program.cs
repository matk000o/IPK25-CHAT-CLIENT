// Martin Knor [xknorm01]
// FIT VUT
// 2025

using System.Net.Sockets;
using System.Net;

namespace Client;

class Program
{
    static void Main(string[] args)
    {
        var options = ClArgumentsParser.Parse(args);
        Console.WriteLine(options.ToString());
    }
}