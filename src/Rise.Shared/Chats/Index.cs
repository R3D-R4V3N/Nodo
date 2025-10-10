<<<<<<< HEAD
using System;

=======
>>>>>>> origin/main
namespace Rise.Shared.Chats;

public static partial class ChatResponse
{
    /// <summary>
    /// Response for listing all chats.
    /// </summary>

    public class Index
    {
<<<<<<< HEAD
        public IEnumerable<ChatDto.Index> Chats { get; set; } = Array.Empty<ChatDto.Index>();
=======
        public IEnumerable<ChatDto.Index> Chats { get; set; }
>>>>>>> origin/main
    }
}