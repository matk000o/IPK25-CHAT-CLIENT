using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Client.Commands;
using Client.Enums;
using Client.Messages;

namespace Client;

public class UdpChatClient : IChatClient
{
    private UdpClient _udpClient;
    private IPEndPoint _remoteEndpoint;
    private readonly CancellationTokenSource _cts = new();
    
    // Retransmission parameters.
    private readonly int _udpTimeout;
    private readonly int _udpMaxRetries;

    // A thread-safe dictionary to manage pending confirmations.
    private readonly ConcurrentDictionary<ushort, TaskCompletionSource<bool>> _pendingConfirmations = new();
    private readonly ConcurrentDictionary<ushort, bool> _processedMessageIds = new();


    public bool Discord { get; set; }
    public string DisplayName { get; set; }
    public ClientState State { get; set; }

    public UdpChatClient(string server, int port, int udpTimeout, int udpMaxRetries, bool discord)
    {
        _udpTimeout = udpTimeout;
        _udpMaxRetries = udpMaxRetries;

        Discord = discord;
        DisplayName = "Unknown";
        State = ClientState.Start;

        _udpClient = new UdpClient(new IPEndPoint(IPAddress.Any, 0));
        IPAddress[] addresses = Dns.GetHostAddresses(server);
        _remoteEndpoint = new IPEndPoint(addresses[0], port);
    }

    public async Task RunAsync()
    {
        try
        {
            MessageBuilder.CurrentClientProtocol = ClientProtocolType.Udp;

            var receiveTask = ReceiveMessagesAsync(_cts.Token);
            var inputTask = HandleUserInputAsync(_cts.Token);

            await Task.WhenAny(receiveTask, inputTask);
            await ShutdownAsync();
            await Task.WhenAll(receiveTask, inputTask);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"ERROR: {ex.Message}");
            Environment.Exit((int)ExitCode.ServerConnectionError);
        }
    }

    /// <summary>
    /// Sends a UDP message and waits for a confirmation with retries.
    /// </summary>
    public async Task SendMessageAsync(byte[] message)
    {
        var messageType = (MessageType)message[0];
        // Pure CONFIRM messages are sent without retries
        if (messageType == MessageType.Confirm)
        {
            await _udpClient.SendAsync(message, message.Length, _remoteEndpoint);
            return;
        }

        // Extract the MessageID
        ushort msgId = (ushort)((message[1] << 8) | message[2]);

        // Prepare a TaskCompletionSource to await the CONFIRM
        var tcs = new TaskCompletionSource<bool>();
        _pendingConfirmations[msgId] = tcs;

        int retries = 0;
        bool confirmed = false;
        while (retries <= _udpMaxRetries && !confirmed)
        {
            await _udpClient.SendAsync(message, message.Length, _remoteEndpoint);

            // Wait either for the confirmation or a timeout
            var timeoutTask = Task.Delay(_udpTimeout);
            var completed = await Task.WhenAny(tcs.Task, timeoutTask);
            if (completed == tcs.Task && tcs.Task.Result)
            {
                confirmed = true;
            }
            else
            {
                retries++;
            }
        }
        // Clean up pending confirmation
        _pendingConfirmations.TryRemove(msgId, out _);

        if (!confirmed)
        {
            Console.WriteLine("ERROR: Message confirmation failed after maximum retries.");

            // If it was an ERR or BYE (terminal) message, exit immediately
            if (messageType is MessageType.Err or MessageType.Bye)
            {
                ExitHandler.Error(ExitCode.TimeOutError);
            }
            await ShutdownAsync();
        }
    }

    private async Task ReceiveMessagesAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                UdpReceiveResult result = await _udpClient.ReceiveAsync(token);
                _remoteEndpoint = result.RemoteEndPoint;

                byte[] data = result.Buffer;
                var message = MessageParser.ParseUdp(data);

                // if it's not a pure Confirm, and we've already seen this ID, just ack & skip
                if (message.Type != MessageType.Confirm)
                {
                    if (!_processedMessageIds.TryAdd(message.MessageId, true))
                    {
                        // send back the confirm so the server knows we got it
                        await SendMessageAsync(MessageBuilder.BuildConfirmMessage(message.MessageId));
                        continue;
                    }
                }

                await ProcessServerMessageAsync(message);
            }
        }
        catch (OperationCanceledException) { /*…*/ }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"ERROR receiving messages: {ex.Message}");
        }
    }

    private async Task ProcessServerMessageAsync(IMessage message)
    {
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
                        Console.WriteLine($"Action Failure: {reply.Content}");
                        State = ClientState.Auth;
                        break;
                    case ClientState.Join:
                        Console.WriteLine(reply.Success
                            ? $"Action Success: {reply.Content}"
                            : $"Action Failure: {reply.Content}");
                        State = ClientState.Open;
                        break;
                    default:
                        await SendErrorMessage("received unexpected REPLY message.");
                        State = ClientState.End;
                        await _cts.CancelAsync();
                        ExitHandler.Error(ExitCode.UnexpectedReplyError);
                        break;
                }
                await SendMessageAsync(MessageBuilder.BuildConfirmMessage(message.MessageId));
                break;
            case MessageType.Msg:
                var chat = (ChatMessage)message;
                Console.WriteLine($"{chat.Sender}: {chat.MessageContent}");
                await SendMessageAsync(MessageBuilder.BuildConfirmMessage(message.MessageId));
                break;
            case MessageType.Err:
                var error = (ErrorMessage)message;
                Console.WriteLine($"ERROR FROM {error.Sender}: {error.ErrorContent}");
                State = ClientState.End;
                await SendMessageAsync(MessageBuilder.BuildConfirmMessage(message.MessageId));
                await ShutdownAsync();
                break;
            case MessageType.Bye:
                var bye = (ByeMessage)message;
                Console.WriteLine($"{bye.Sender} has disconnected.");
                State = ClientState.End;
                await SendMessageAsync(MessageBuilder.BuildConfirmMessage(message.MessageId));
                await ShutdownAsync();
                break;
            case MessageType.Confirm:
                var confirm = (ConfirmMessage)message;
                if (_pendingConfirmations.TryGetValue(confirm.MessageId, out var tcs))
                {
                    tcs.TrySetResult(true);
                    // Console.WriteLine($"Received CONFIRM for message {confirm.MessageId}");
                }
                break;
            case MessageType.Ping:
                await SendMessageAsync(MessageBuilder.BuildConfirmMessage(message.MessageId));
                break;
                
            default:
                await SendMessageAsync(MessageBuilder.BuildConfirmMessage(message.MessageId));
                await SendErrorMessage("ERROR: Received unknown message type.");
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
                    await SendMessageAsync(MessageBuilder.BuildByeMessage(DisplayName));
                    break;
                }

                var command = CommandFactory.GetCommand(input);
                if (command != null)
                {
                    // Ideally, refactor your commands to accept IClient.
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
            // Expected.
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"ERROR processing input: {ex.Message}");
        }
    }

        private async Task SendErrorMessage(string error)
    {
        Console.WriteLine($"ERROR: {error}");
        await SendMessageAsync(MessageBuilder.BuildErrorMessage(DisplayName, error));
    }
    
    private static async Task<string?> ReadLineAsync(CancellationToken token)
    {
        var inputTask = Task.Run(Console.ReadLine, token);
        var cancelTask = Task.Delay(Timeout.Infinite, token);
        var completed = await Task.WhenAny(inputTask, cancelTask);
        if (completed == cancelTask)
            throw new OperationCanceledException(token);
        return await inputTask;
    }

    public async Task ShutdownAsync()
    {
        if (State != ClientState.End)
        {
            await SendMessageAsync(MessageBuilder.BuildByeMessage(DisplayName));
            State = ClientState.End;
        }
        _cts.Cancel();
        _udpClient.Close();
    }
}
