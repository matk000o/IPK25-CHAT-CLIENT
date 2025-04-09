namespace Client.Enums;

public enum MessageType : byte
{
    Confirm = 0x00,
    Reply   = 0x01,
    Auth    = 0x02,
    Join    = 0x03,
    Msg     = 0x04,
    Ping    = 0xFD,
    Err     = 0xFE,
    Bye     = 0xFF
}