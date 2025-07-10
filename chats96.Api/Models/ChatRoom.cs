using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace chats96.Api.Models
{
    public class ChatRoom
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // We generate the key, not DB
        public string ChatRoomKey { get; set; } = Guid.NewGuid().ToString("N");

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property for messages (EF Core will manage the relationship)
        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();

        // Property to track active user count (denormalized, updated via SignalR hub)
        public int ActiveUsers { get; set; } = 0;

        // Last activity timestamp for cleanup
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;
    }
}