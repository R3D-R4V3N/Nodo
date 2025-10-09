using System;

namespace Rise.Shared.Chats;

public static partial class ChatResponse
{
    /// <summary>
    /// Response for listing all chats.
    /// </summary>

    public class Index
    {
        public IEnumerable<ChatDto.Index> Chats { get; set; } = Array.Empty<ChatDto.Index>();
    }
}