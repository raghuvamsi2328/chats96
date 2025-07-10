using System;
using System.ComponentModel.DataAnnotations;

namespace chats96.Api.Models
{
    public class ChatMessage
    {
        public int Id { get; set; } // Primary Key

        [Required]
        [MaxLength(100)]
        public string Sender { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string MessageContent { get; set; } = string.Empty; // Changed to avoid conflict with class name

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Foreign Key to ChatRoom
        [Required]
        public string ChatRoomKey { get; set; } = string.Empty;
        public ChatRoom? ChatRoom { get; set; } // Navigation property
    }
}