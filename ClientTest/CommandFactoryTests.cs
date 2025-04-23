using Client.Commands;

namespace ClientTest;

public class CommandFactoryTests
{
    [Theory]
    [InlineData("/auth user secret name", typeof(AuthCommand))]
    [InlineData("/join mychan", typeof(JoinCommand))]
    [InlineData("/bye", typeof(ByeCommand))]
    [InlineData("/help", typeof(HelpCommand))]
    [InlineData("hello there", typeof(ChatCommand))]
    public void GetCommand_VariousInputs_ReturnsExpected(string input, Type expectedType)
    {
        var cmd = CommandFactory.GetCommand(input);
        Assert.NotNull(cmd);
        Assert.IsType(expectedType, cmd);
    }

    [Fact]
    public void GetCommand_InvalidSlash_ReturnsNull()
    {
        var cmd = CommandFactory.GetCommand("/unknown");
        Assert.Null(cmd);
    }
}