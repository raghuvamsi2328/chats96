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

        // PIN protection for the room (optional)
        [MaxLength(10)]
        public string? RoomPin { get; set; } = null;

        // Room expiry date (optional - if null, room doesn't expire)
        public DateTime? ExpiresAt { get; set; } = null;

        // Room title/name for better UX
        [MaxLength(100)]
        public string? RoomTitle { get; set; } = null;

        // Creator's name for room management
        [MaxLength(100)]
        public string? CreatedBy { get; set; } = null;

        // Flag to indicate if room should persist even when empty
        public bool IsPersistent { get; set; } = false;
    }
}