using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using chats96.Api.Data;
using chats96.Api.Models;
using System;
using System.Threading.Tasks;

namespace chats96.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ChatController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateChatRoom([FromBody] CreateRoomRequest request)
        {
            try
            {
                // Validate expiry date
                if (request.ExpiresAt.HasValue && request.ExpiresAt.Value <= DateTime.UtcNow)
                {
                    return BadRequest(new { error = "Expiry date must be in the future" });
                }

                // Generate a unique key for the new chat room
                string chatRoomKey = Guid.NewGuid().ToString("N");

                var chatRoom = new ChatRoom
                {
                    ChatRoomKey = chatRoomKey,
                    RoomTitle = request.RoomTitle,
                    CreatedBy = request.CreatedBy,
                    RoomPin = string.IsNullOrWhiteSpace(request.RoomPin) ? null : request.RoomPin,
                    ExpiresAt = request.ExpiresAt,
                    IsPersistent = request.IsPersistent || !string.IsNullOrWhiteSpace(request.RoomPin) || request.ExpiresAt.HasValue,
                    CreatedAt = DateTime.UtcNow,
                    LastActivity = DateTime.UtcNow
                };

                _context.ChatRooms.Add(chatRoom);
                await _context.SaveChangesAsync();

                return Ok(new { 
                    key = chatRoomKey,
                    title = chatRoom.RoomTitle,
                    requiresPin = !string.IsNullOrWhiteSpace(chatRoom.RoomPin),
                    expiresAt = chatRoom.ExpiresAt,
                    isPersistent = chatRoom.IsPersistent
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to create chat room", details = ex.Message });
            }
        }

        [HttpGet("room-info/{chatRoomKey}")]
        public async Task<IActionResult> GetRoomInfo(string chatRoomKey)
        {
            try
            {
                var chatRoom = await _context.ChatRooms
                    .FirstOrDefaultAsync(cr => cr.ChatRoomKey == chatRoomKey);

                if (chatRoom == null)
                {
                    return NotFound(new { error = "Chat room not found" });
                }

                bool isExpired = chatRoom.ExpiresAt.HasValue && chatRoom.ExpiresAt.Value <= DateTime.UtcNow;

                var response = new RoomInfoResponse
                {
                    ChatRoomKey = chatRoom.ChatRoomKey,
                    RoomTitle = chatRoom.RoomTitle,
                    CreatedBy = chatRoom.CreatedBy,
                    CreatedAt = chatRoom.CreatedAt,
                    ExpiresAt = chatRoom.ExpiresAt,
                    RequiresPin = !string.IsNullOrWhiteSpace(chatRoom.RoomPin),
                    IsExpired = isExpired,
                    ActiveUsers = chatRoom.ActiveUsers,
                    LastActivity = chatRoom.LastActivity
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to get room info", details = ex.Message });
            }
        }

        [HttpPost("verify-pin")]
        public async Task<IActionResult> VerifyRoomPin([FromBody] JoinRoomRequest request)
        {
            try
            {
                var chatRoom = await _context.ChatRooms
                    .FirstOrDefaultAsync(cr => cr.ChatRoomKey == request.ChatRoomKey);

                if (chatRoom == null)
                {
                    return NotFound(new { error = "Chat room not found" });
                }

                // Check if room is expired
                if (chatRoom.ExpiresAt.HasValue && chatRoom.ExpiresAt.Value <= DateTime.UtcNow)
                {
                    return BadRequest(new { error = "This chat room has expired" });
                }

                // Check PIN if required
                if (!string.IsNullOrWhiteSpace(chatRoom.RoomPin))
                {
                    if (request.RoomPin != chatRoom.RoomPin)
                    {
                        return Unauthorized(new { error = "Invalid PIN" });
                    }
                }

                return Ok(new { 
                    success = true, 
                    message = "Access granted",
                    roomTitle = chatRoom.RoomTitle 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to verify PIN", details = ex.Message });
            }
        }
    }
}