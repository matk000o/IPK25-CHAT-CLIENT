using System.Net;
using System.Net.Sockets;
using System.Text;
using Client.Commands;
using Client.Enums;
namespace Client.Tcp;

public class TcpChatClient(string server, int port)
{
    private IClientCommand _currentCommand = new EmptyCommand();
    private FsmState _currentState = FsmState.Start;
    private Socket? _socket;
    private NetworkStream? _stream;
    private StreamReader? _reader;
    private StreamWriter? _writer;

    public async Task RunAsync()
    {
        try
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await _socket.ConnectAsync(server, port);

            Console.WriteLine($"Connected to {server}:{port}");

            _stream = new NetworkStream(_socket);
            _reader = new StreamReader(_stream, Encoding.ASCII);
            _writer = new StreamWriter(_stream, Encoding.ASCII) { AutoFlush = true };

            while (true)
            {
                var input = Console.ReadLine();
                if (input is null)
                {
                    break;
                }
                if (string.IsNullOrWhiteSpace(input))
                {
                    continue;
                }
                try
                {
                    _currentCommand = CommandFactory.ParseInput(input);
                    await ExecuteCommand();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Command error: {ex.Message}");
                }   
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            Cleanup();
        }
    }

    private async Task ExecuteCommand()
    {
        switch (_currentState)
        {
            case FsmState.Start:
                if (_currentCommand.Type is not CommandType.Auth)
                    throw new Exception("Command is not supported in this FsmState");
                string message = ConstructProtocolMessage(_currentCommand);
                Console.WriteLine("Client sending: " + message.TrimEnd());
                await _writer.WriteAsync(message);
                
                // Read response
                string? response = await _reader.ReadLineAsync();
                Console.WriteLine("Server returned: " + response);
                break;
        }
    }


    private string ConstructProtocolMessage(IClientCommand currentCommand)
    {
        string protocolMessage;
        switch (currentCommand.Type)
        {
            case CommandType.Auth:
                var authCommand = (AuthCommand)currentCommand;
                protocolMessage = $"AUTH {authCommand.Username} AS {authCommand.DisplayName} USING {authCommand.Secret}\r\n";
                break;
            default:
                throw new Exception($"Unknown command: {currentCommand.Type}");
        }
        return protocolMessage;
    }

    
    private void Cleanup()
    {
        _reader?.Close();
        _writer?.Close();
        _stream?.Close();
        _socket?.Close();
    }
}