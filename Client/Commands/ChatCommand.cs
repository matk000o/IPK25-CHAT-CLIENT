using Client.Enums;
using Client.Messages;

namespace Client.Commands;

public class ChatCommand : Command
{
    public override async Task ExecuteAsync(TcpChatClient client, string chatMessage)
    {
        if (client.State == ClientState.Open)
        {
            if (chatMessage.Length > 60_000)
            {
                Console.WriteLine("ERROR: chat message is too long and must be truncated");
                chatMessage = chatMessage[..60_000];
            }
            // Build the chat message according to the protocol.
            byte[] message = MessageBuilder.BuildChatMessage(client.DisplayName, chatMessage);
            await client.SendMessageAsync(message);
        }
        else
        {
            Console.WriteLine("ERROR: Cannot send messages in the current state.");
        }
    }
}