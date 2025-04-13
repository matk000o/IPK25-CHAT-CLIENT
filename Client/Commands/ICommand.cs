namespace Client.Commands;

public interface ICommand
{
    Task ExecuteAsync(TcpChatClient client, string[] args);
}