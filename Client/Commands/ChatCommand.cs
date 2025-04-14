using Client.Enums;
using Client.Messages;

namespace Client.Commands;

public class ChatCommand : Command
{
    public override async Task ExecuteAsync(TcpChatClient client, string chatMessage)
    {
        if (client.State == ClientState.Open)
        {
            // Build the chat message according to the protocol.
            string messageToSend = MessageFactory.BuildChatMessage(client.DisplayName, chatMessage);
            await client.SendMessageAsync(messageToSend);
        }
        else
        {
            Console.WriteLine("ERROR: Cannot send messages in the current state.");
        }
    }
}