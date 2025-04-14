using System.Net.Sockets;
using System.Text;
using Client.Enums;
using Client.Messages;
using Client.Commands;

namespace Client;

public class TcpChatClient
{
    // These properties are part of the public API for command handling.

    private readonly string _server;
    private readonly int _port;
    private ChatConnection _connection;
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();

    public readonly bool Discord;
    public string DisplayName { get; set; }
    public ClientState State { get; set; }

    public TcpChatClient(string server, int port, bool discord)
    {
        _server = server;
        _port = port;
        Discord = discord;
        DisplayName = "Anonymous";
        State = ClientState.Start;
    }

    public async Task RunAsync()
    {
        try
        {
            await ConnectAsync();

            // Start concurrently receiving server messages and processing user input.
            var receivingTask = ReceiveMessagesAsync(_cts.Token);
            var inputTask = HandleUserInputAsync(_cts.Token);

            // Wait until one of the tasks finishes.
            await Task.WhenAny(receivingTask, inputTask);

            // Shutdown and cancel any remaining operations.
            await ShutdownAsync();
            await Task.WhenAll(receivingTask, inputTask);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"ERROR: {ex.Message}");
            Environment.Exit(1);
        }
    }

    private async Task ConnectAsync()
    {
        var tcpClient = new TcpClient();
        await tcpClient.ConnectAsync(_server, _port);
        _connection = new ChatConnection(tcpClient);
        Console.WriteLine($"Connected to {_server}:{_port}");
    }

    private async Task ReceiveMessagesAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                string? line = await _connection.ReadMessageAsync(token);
                if (line == null)
                {
                    Console.WriteLine("Server closed connection.");
                    State = ClientState.End;
                    await _cts.CancelAsync();
                    break;
                }
                await ProcessServerMessage(line);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected on cancellation; do nothing.
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"ERROR: {ex.Message}");
            State = ClientState.End;
            await _cts.CancelAsync();
        }
    }

    private async Task ProcessServerMessage(string rawMessage)
    {
        // Parse the incoming message using the dedicated MessageParser.
        var message = MessageParser.Parse(rawMessage);
        switch (message.Type)
        {
            case MessageType.Reply:
                var reply = (ReplyMessage)message;
                switch (State)
                {
                    case ClientState.Auth when reply.Success:
                        Console.WriteLine($"Action Success: {reply.Content}");
                        State = ClientState.Open;
                        break;
                    case ClientState.Auth:
                        Console.WriteLine($"Action Fail: {reply.Content}");
                        State = ClientState.Auth;
                        break;
                    case ClientState.Join:
                        Console.WriteLine(reply.Success
                            ? $"Action Success: {reply.Content}"
                            : $"Action Fail: {reply.Content}");
                        State = ClientState.Open;
                        break;
                    default:
                        await SendErrorMessage("received unexpected REPLY message.");
                        State = ClientState.End;
                        await _cts.CancelAsync();
                        ExitHandler.Error(ExitCode.UnexpectedReplyError);
                        break;
                }
                break;

            case MessageType.Chat:
                var chat = (ChatMessage)message;
                Console.WriteLine($"{chat.Sender}: {chat.MessageContent}");
                break;

            case MessageType.Error:
                var error = (ErrorMessage)message;
                Console.WriteLine($"ERROR FROM {error.Sender}: {error.ErrorContent}");
                State = ClientState.End;
                await _cts.CancelAsync();
                break;

            case MessageType.Bye:
                var bye = (ByeMessage)message;
                Console.WriteLine($"{bye.Sender} has disconnected.");
                State = ClientState.End;
                await _cts.CancelAsync();
                break;

            case MessageType.Unknown:
            default:
                await SendErrorMessage("received malformed message form the server.");
                State = ClientState.End;
                await _cts.CancelAsync();
                ExitHandler.Error(ExitCode.MalformedMsgError);
                break;
        }
    }

    private async Task HandleUserInputAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                string? input = await ReadLineAsync(token);
                if (input == null)
                {
                    await SendMessageAsync(MessageFactory.BuildByeMessage(DisplayName));
                    break;
                }
                
                // Delegate command processing to a CommandFactory.
                var command = CommandFactory.GetCommand(input);
                if (command != null)
                {
                    await command.ExecuteAsync(this, input);
                }
                else
                {
                    Console.WriteLine("ERROR: Unknown command");
                }            
            }
        }
        catch (OperationCanceledException)
        {
            // Expected cancellation.
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"ERROR processing input: {ex.Message}");
            State = ClientState.End;
            await _cts.CancelAsync();
        }
    }

    // Console.ReadLine is blocking function and without this helper function
    // the Console.ReadLine would leve the program hanging when trying to exit  
    private static async Task<string?> ReadLineAsync(CancellationToken token)
    {
        // Wrap Console.ReadLine in a task and race against a cancellation delay.
        var inputTask = Task.Run(Console.ReadLine, token);
        var cancelTask = Task.Delay(Timeout.Infinite, token);
        var completed = await Task.WhenAny(inputTask, cancelTask);
        if (completed == cancelTask)
            throw new OperationCanceledException(token);
        return await inputTask;
    }
    
    public async Task SendMessageAsync(string message)
    {
        // Append CRLF as required by the protocol.
        await _connection.WriteAsync(message + "\r\n");
    }
    
    private async Task SendErrorMessage(string error)
    {
        Console.WriteLine($"ERROR: {error}");
        await SendMessageAsync(MessageFactory.BuildErrorMessage(DisplayName, error));
    }

    public async Task ShutdownAsync()
    {
        if (State != ClientState.End)
        {
            await SendMessageAsync(MessageFactory.BuildByeMessage(DisplayName));
            State = ClientState.End;
        }
        await _cts.CancelAsync();
    }
}

// ChatConnection encapsulates the low-level I/O.
public class ChatConnection
{
    private readonly StreamReader _reader;
    private readonly StreamWriter _writer;

    public ChatConnection(TcpClient tcpClient)
    {
        var stream = tcpClient.GetStream();
        _reader = new StreamReader(stream);
        _writer = new StreamWriter(stream) { AutoFlush = true };
    }

    // New method to read until the delimiter "\r\n" is encountered.
    public async Task<string?> ReadMessageAsync(CancellationToken token)
    {
        var sb = new StringBuilder();
        char[] buffer = new char[1];
        int previousChar = -1;
        
        while (!token.IsCancellationRequested)
        {
            int read = await _reader.ReadAsync(buffer, 0, 1);
            if (read == 0)
            {
                // End of stream.
                break;
            }
            char currentChar = buffer[0];
            sb.Append(currentChar);
            
            if (previousChar == '\r' && currentChar == '\n')
            {
                // Remove the trailing "\r\n" from the message.
                return sb.ToString(0, sb.Length - 2);
            }
            
            previousChar = currentChar;
        }
        
        // If there is content but no terminator, you may decide to return it,
        // or treat it as incomplete; here we return it if any.
        return sb.Length > 0 ? sb.ToString() : null;
    }

    public async Task WriteAsync(string message) => await _writer.WriteAsync(message);
}

