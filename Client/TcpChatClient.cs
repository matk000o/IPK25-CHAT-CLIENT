using System.Net.Sockets;
using Client.Enums;
using Client.Messages;
using Client.Commands;

namespace Client;

public class TcpChatClient
{
    // These properties are part of the public API for command handling.
    public string DisplayName { get; set; }
    public ClientState State { get; set; }

    private readonly string _server;
    private readonly int _port;
    private ChatConnection _connection;
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();

    public TcpChatClient(string server, int port)
    {
        _server = server;
        _port = port;
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

    public async Task SendMessageAsync(string message)
    {
        // Append CRLF as required by the protocol.
        await _connection.WriteAsync(message + "\r\n");
    }

    private async Task ReceiveMessagesAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                string? line = await _connection.ReadLineAsync();
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
            Console.Error.WriteLine($"ERROR receiving messages: {ex.Message}");
            State = ClientState.End;
            await _cts.CancelAsync();
        }
    }

    private async Task ProcessServerMessage(string rawMessage)
    {
        // Parse the incoming message using the dedicated MessageParser.
        IMessage message = MessageParser.Parse(rawMessage);
        switch (message.Type)
        {
            case MessageType.Reply:
                var reply = (ReplyMessage)message;
                if (State == ClientState.Auth)
                {
                    if (reply.Success)
                    {
                        Console.WriteLine($"Action Success: {reply.Content}");
                        State = ClientState.Open;
                    }
                    else
                    {
                        Console.WriteLine($"Action Fail: {reply.Content}");
                        State = ClientState.End;
                        await _cts.CancelAsync();
                    }
                }
                else if (State == ClientState.Join)
                {
                    Console.WriteLine(reply.Success
                        ? $"Action Success: {reply.Content}"
                        : $"Action Fail: {reply.Content}");
                    State = ClientState.Open;
                }
                else
                {
                    await SendErrorMessage("Unexpected REPLY message.");
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

            default:
                Console.WriteLine("Received: " + rawMessage);
                break;
        }
    }

    private async Task SendErrorMessage(string error)
    {
        Console.WriteLine($"ERROR: {error}");
        await SendMessageAsync($"ERR FROM {DisplayName} IS {error}");
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
                    await SendMessageAsync($"BYE FROM {DisplayName}");
                    break;
                }

                if (input.StartsWith("/"))
                {
                    await ProcessCommandAsync(input);
                }
                else
                {
                    if (State == ClientState.Open)
                    {
                        await SendMessageAsync($"MSG FROM {DisplayName} IS {input}");
                    }
                    else
                    {
                        Console.WriteLine("ERROR: Cannot send messages in the current state.");
                    }
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

    private async Task<string?> ReadLineAsync(CancellationToken token)
    {
        // Wrap Console.ReadLine in a task and race against a cancellation delay.
        var inputTask = Task.Run(() => Console.ReadLine());
        var cancelTask = Task.Delay(Timeout.Infinite, token);
        var completed = await Task.WhenAny(inputTask, cancelTask);
        if (completed == cancelTask)
            throw new OperationCanceledException(token);
        return await inputTask;
    }

    private async Task ProcessCommandAsync(string commandLine)
    {
        var parts = commandLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return;

        string commandName = parts[0].ToLower();
        // Delegate command processing to a CommandFactory.
        ICommand? command = CommandFactory.GetCommand(commandName);
        if (command != null)
        {
            await command.ExecuteAsync(this, parts);
        }
        else if (commandName == "/rename")
        {
            if (parts.Length < 2)
            {
                Console.WriteLine("ERROR: /rename requires a new display name");
                return;
            }
            DisplayName = parts[1];
            Console.WriteLine($"Display name changed to {DisplayName}");
        }
        else if (commandName == "/help")
        {
            PrintHelp();
        }
        else
        {
            Console.WriteLine("ERROR: Unknown command");
        }
    }

    private static void PrintHelp()
    {
        Console.WriteLine("Supported commands:");
        Console.WriteLine("/auth <username> <secret> <displayName> - Authenticate to the server.");
        Console.WriteLine("/join <channelId> - Join a channel.");
        Console.WriteLine("/rename <displayName> - Change your display name.");
        Console.WriteLine("/help - Show this help message.");
        Console.WriteLine("/bye - Disconnect from the server.");
    }

    public async Task ShutdownAsync()
    {
        if (State != ClientState.End)
        {
            await SendMessageAsync($"BYE FROM {DisplayName}");
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

    public async Task<string?> ReadLineAsync() => await _reader.ReadLineAsync();
    public async Task WriteAsync(string message) => await _writer.WriteAsync(message);
}

