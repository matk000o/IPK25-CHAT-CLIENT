using System.Net.Sockets;
using System.Text.RegularExpressions;
using Client.Enums;

namespace Client
{
    public class TcpChatClient(string server, int port)
    {
        private TcpClient _tcpClient;
        private StreamReader _reader;
        private StreamWriter _writer;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private ClientState _state = ClientState.Start;
        private string _displayName = string.Empty;

        // Initially, before any connection, we are in the Start state.

        public async Task RunAsync()
        {
            try
            {
                await ConnectAsync();

                // Start concurrently reading from the network and handling user input.
                var receivingTask = ReceiveMessagesAsync(_cts.Token);
                var sendingTask = HandleUserInputAsync(_cts.Token);

                // Wait until one of the tasks finishes (for example, on error or termination).
                await Task.WhenAny(receivingTask, sendingTask);

                // Cancel the other task and wait for both to complete.
                await _cts.CancelAsync();
                await Task.WhenAll(receivingTask, sendingTask);
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync("ERROR: " + ex.Message);
                Environment.Exit(1);
            }
        }

        private async Task ConnectAsync()
        {
            
            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(server, port);
            var networkStream = _tcpClient.GetStream();

            _reader = new StreamReader(networkStream);
            _writer = new StreamWriter(networkStream) { AutoFlush = true };

            Console.WriteLine($"Connected to {server}:{port}");
        }

        private async Task ReceiveMessagesAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    string? line = await _reader.ReadLineAsync();
                    if (line == null)
                    {
                        Console.WriteLine("Server closed connection.");
                        await SendByeMessage();
                        _state = ClientState.End;
                        await _cts.CancelAsync();
                        break;
                    }
                    await ProcessServerMessage(line);
                }
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync("ERROR receiving messages: " + ex.Message);
                _state = ClientState.End;
                await _cts.CancelAsync();
            }
        }

        private async Task ProcessServerMessage(string message)
        {
            // Handle REPLY messages, which are expected for both /auth and /join operations.
            if (message.StartsWith("REPLY"))
            {
                // Expected format examples:
                // "REPLY OK IS Auth success." or "REPLY NOK IS Some error message."
                Regex replyPattern = new(@"^REPLY\s(OK|NOK)\sIS\s([\x20-\x7E\n]{1,60000})$", RegexOptions.Compiled);
                var match = replyPattern.Match(message);
                if (!match.Success)
                    await SendErrMessage("Received wrong REPLY message format from the server.");
        
                bool replyValue = match.Groups[1].Value == "OK";
                string responseContent = match.Groups[2].Value;
                switch (_state)
                {
                    case ClientState.Auth:
                        if (replyValue)
                        {
                            Console.WriteLine($"Action Success: {responseContent}");
                            _state = ClientState.Open;
                        }
                        break;
                    case ClientState.Join:
                        Console.WriteLine(replyValue
                            ? $"Action Success: {responseContent}"
                            : $"Action Fail: {responseContent}");
                        _state = ClientState.Open;
                        break;
                    default:
                        await SendErrMessage("received REPLY message from the server while in a state that doesn't expect it");
                        break;                
                }
            }
            else if (message.StartsWith("MSG"))
            {
                // Expected format: "MSG FROM {DisplayName} IS {MessageContent}"
                Regex replyPattern = new(@"^MSG\sFROM\s([\x21-\x7E]{1,20})\sIS\s([\x20-\x7E\n]{1,60000})$", RegexOptions.Compiled);
                var match = replyPattern.Match(message);
                if (!match.Success)
                    await SendErrMessage("Received wrong MSG message format from the server.");
                if (_state == ClientState.Auth)
                    await SendErrMessage("received MSG message from the server while in a state that doesn't expect it");
                else
                {
                    string sender = match.Groups[1].Value;
                    string msgContent = match.Groups[2].Value;
                    Console.WriteLine($"{sender}: {msgContent}");
                }

            }
            else if (message.StartsWith("ERR"))
            {
                // Expected format: "ERR FROM {DisplayName} IS {MessageContent}"
                Regex replyPattern = new(@"^ERR\sFROM\s([\x21-\x7E]{1,20})\sIS\s([\x20-\x7E\n]{1,60000})$", RegexOptions.Compiled);
                var match = replyPattern.Match(message);
                if (!match.Success)
                    await SendErrMessage("Received wrong ERR message format from the server.");
                string sender = match.Groups[1].Value;
                string errContent = match.Groups[2].Value;
                Console.WriteLine($"ERROR FROM {sender}: {errContent}");
                _state = ClientState.End;
                await _cts.CancelAsync();
            }
            else if (message.StartsWith("BYE"))
            {
                // Expected format: "BYE FROM {DisplayName}"
                Regex replyPattern = new(@"^BYE\sFROM\s([\x21-\x7E]{1,20})$", RegexOptions.Compiled);
                var match = replyPattern.Match(message);
                if (!match.Success)
                    await SendErrMessage("Received wrong BYE message format from the server.");
                string sender = match.Groups[1].Value;
                Console.WriteLine($"{sender} has disconnected.");
                _state = ClientState.End;
                await _cts.CancelAsync();
            }
            else
            {
                // For any unrecognized message.
                Console.WriteLine("Received: " + message);
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
                        await SendByeMessage();
                        break;
                    }

                    if (input.StartsWith('/'))
                    {
                        // Process command input (e.g., /auth, /join, /rename, /help, /bye).
                        await ProcessCommand(input);
                    }
                    else
                    {
                        // Process as a chat message if in an appropriate state.
                        if (_state == ClientState.Open)
                        {
                            await SendChatMessage(input);
                        }
                        else
                        {
                            Console.WriteLine("ERROR: Cannot send messages in the current state.");
                        }
                    }
                }
            }
            catch (OperationCanceledException){}
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync("ERROR processing input: " + ex.Message);
                _state = ClientState.End;
                await _cts.CancelAsync();
            }
        }

        private async Task<string?> ReadLineAsync(CancellationToken token)
        {
            // Start a task to read from the console.
            var inputTask = Task.Run(() => Console.ReadLine());
            // Create a task that completes when cancellation is signaled.
            var cancelTask = Task.Delay(Timeout.Infinite, token);

            // Wait for either to complete.
            var completed = await Task.WhenAny(inputTask, cancelTask);

            if (completed == cancelTask)
            {
                // Cancellation was requested.
                throw new OperationCanceledException(token);
            }
            else
            {
                return await inputTask;
            }
        }
        
        private async Task ProcessCommand(string commandLine)
        {
            // Split the command into parts.
            var parts = commandLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var command = parts[0];
            
            switch (command)
            {
                case "/auth":
                    if (_state != ClientState.Start && _state != ClientState.Auth)
                    {
                        Console.WriteLine($"ERROR: Authentication can only be initiated " +
                                          $"from the Start/Auth state. Current state: {_state}");
                        return;
                    }
                    if (parts.Length < 4)
                    {
                        Console.WriteLine("ERROR: usage: /auth {username} {secret} {displayName}");
                        return;
                    }
                    string username = parts[1];
                    string secret = parts[2];
                    string newDisplayName = parts[3];

                    // Set the new display name.
                    _displayName = newDisplayName;
                    // Transition to Auth state.
                    _state = ClientState.Auth;
                    // Build the AUTH message: "AUTH {username} AS {displayName} USING {secret}\r\n"
                    string authMsg = $"AUTH {username} AS {_displayName} USING {secret}";
                    await SendMessage(authMsg);
                    break;

                case "/join":
                    if (_state != ClientState.Open)
                    {
                        Console.WriteLine("ERROR: You must be in the Open state to join a channel.");
                        return;
                    }
                    if (parts.Length < 2)
                    {
                        Console.WriteLine("ERROR: usage: /join {channelId}");
                        return;
                    }
                    string channelId = parts[1];
                    // Transition to Join state.
                    _state = ClientState.Join;
                    // Build the JOIN message: "JOIN {channelId} AS {displayName}\r\n"
                    string joinMsg = $"JOIN {channelId} AS {_displayName}";
                    await SendMessage(joinMsg);
                    break;

                case "/rename":
                    if (parts.Length < 2)
                    {
                        Console.WriteLine("ERROR: /rename requires a new display name");
                        return;
                    }
                    string newName = parts[1];
                    _displayName = newName;
                    Console.WriteLine($"Display name changed to {_displayName}");
                    break;

                case "/help":
                    PrintHelp();
                    break;

                case "/bye":
                    await SendByeMessage();
                    _state = ClientState.End;
                    await _cts.CancelAsync();
                    break;

                default:
                    Console.WriteLine("ERROR: Unknown command");
                    break;
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

        private async Task SendChatMessage(string message)
        {
            // Build chat message: "MSG FROM {displayName} IS {message}\r\n"
            string chatMsg = $"MSG FROM {_displayName} IS {message}";
            await SendMessage(chatMsg);
        }

        private async Task SendByeMessage()
        {
            // Build BYE message: "BYE FROM {displayName}\r\n"
            string byeMsg = $"BYE FROM {_displayName}";
            await SendMessage(byeMsg);
        }        
        private async Task SendErrMessage(string message)
        {
            Console.WriteLine($"ERROR: {message}");
            // Build err message: "ERR FROM {displayName} IS {message}\r\n"
            string errMsg = $"ERR FROM {_displayName} IS {message}";
            await SendMessage(errMsg);
        }

        private async Task SendMessage(string message)
        {
            // Append CRLF as required by the protocol.
            string fullMessage = message + "\r\n";
            await _writer.WriteAsync(fullMessage);
        }
        public async Task ShutdownAsync()
        {
            if (_state != ClientState.End)
            {
                await SendByeMessage();
                _state = ClientState.End;
            }
            await _cts.CancelAsync();
        }
    }
}
