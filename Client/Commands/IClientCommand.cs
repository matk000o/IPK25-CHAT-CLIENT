using Client.Enums;

namespace Client.Commands;

public interface IClientCommand
{
    CommandType Type { get; }
    void Parse(string[] args);
}