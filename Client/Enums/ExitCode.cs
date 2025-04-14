namespace Client.Enums;

public enum ExitCode
{
    Success = 0,
    CommandLineError = 11,
    ServerConnectionError = 21,
    MalformedMsgError = 31,
    TimeOutError = 32,
    NoReplyError = 33,
    UnexpectedReplyError = 34,
}