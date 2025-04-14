namespace Client.Messages;

public static class MessageFactory
{
    /// <summary>
    /// Builds an authentication message.
    /// Format: AUTH {username} AS {displayName} USING {secret}
    /// </summary>
    /// <param name="username">User ID</param>
    /// <param name="displayName">Display name</param>
    /// <param name="secret">The secret (e.g. password)</param>
    /// <returns>The constructed authentication message.</returns>
    public static string BuildAuthMessage(string username, string displayName, string secret) =>
        $"AUTH {username} AS {displayName} USING {secret}";

    /// <summary>
    /// Builds a join message.
    /// Format: JOIN {channelId} AS {displayName}
    /// If a Discord connection is enabled, a "discord." prefix is added to the channelId.
    /// </summary>
    public static string BuildJoinMessage(string channelId, string displayName, bool useDiscord) =>
        useDiscord 
            ? $"JOIN discord.{channelId} AS {displayName}" 
            : $"JOIN {channelId} AS {displayName}";

    /// <summary>
    /// Builds a chat message.
    /// Format: MSG FROM {displayName} IS {content}
    /// </summary>
    public static string BuildChatMessage(string displayName, string content) =>
        $"MSG FROM {displayName} IS {content}";

    /// <summary>
    /// Builds a bye message.
    /// Format: BYE FROM {displayName}
    /// </summary>
    public static string BuildByeMessage(string displayName) =>
        $"BYE FROM {displayName}";

    /// <summary>
    /// Builds an error message.
    /// Format: ERR FROM {displayName} IS {error}
    /// </summary>
    public static string BuildErrorMessage(string displayName, string error) =>
        $"ERR FROM {displayName} IS {error}";
}