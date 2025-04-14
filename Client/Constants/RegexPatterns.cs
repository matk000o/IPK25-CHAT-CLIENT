namespace Client.Constants;

// implements this part of the protocols ABNF:

// ID        = 1*20    ( ALPHA / DIGIT / "_" / "-" )
// SECRET    = 1*128   ( ALPHA / DIGIT / "_" / "-" )
// CONTENT   = 1*60000 ( VCHAR / SP / LF )
// DNAME     = 1*20    VCHAR

public static class RegexPatterns
{
    // ID: 1 to 20 characters of ALPHA / DIGIT / "_" / "-"
    public const string IdPattern = @"[A-Za-z0-9_-]{1,20}";

    // SECRET: 1 to 128 characters of ALPHA / DIGIT / "_" / "-"
    public const string SecretPattern = @"[A-Za-z0-9_-]{1,128}";

    // CONTENT: 1 to 60000 characters of VCHAR (0x21-0x7E), SP (0x20) or LF (\n)
    // Note: VCHAR is [\x21-\x7E]; include space (\x20) and LF (\n)
    public const string ContentPattern = @"[\x20-\x7E\n]{1,60000}";

    // DNAME: 1 to 20 VCHAR (visible characters, hex 21-7E)
    public const string DNamePattern = @"[\x21-\x7E]{1,20}";
}