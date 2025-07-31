using System;
using System.ComponentModel.DataAnnotations;

namespace chats96.Api.Models
{
    public class CreateRoomRequest
    {
        [Required]
        [MaxLength(100)]
        public string RoomTitle { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string CreatedBy { get; set; } = string.Empty;

        [MaxLength(10)]
        public string? RoomPin { get; set; } = null;

        public DateTime? ExpiresAt { get; set; } = null;

        public bool IsPersistent { get; set; } = false;
    }

    public class JoinRoomRequest
    {
        [Required]
        public string ChatRoomKey { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string UserName { get; set; } = string.Empty;

        public string? RoomPin { get; set; } = null;
    }

    public class RoomInfoResponse
    {
        public string ChatRoomKey { get; set; } = string.Empty;
        public string? RoomTitle { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool RequiresPin { get; set; }
        public bool IsExpired { get; set; }
        public int ActiveUsers { get; set; }
        public DateTime LastActivity { get; set; }
    }
}
