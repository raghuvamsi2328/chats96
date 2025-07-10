using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace SimpleChatApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // This will be /api/chat
    public class ChatController : ControllerBase
    {
        [HttpPost("create")]
        public IActionResult CreateChatRoom()
        {
            // Generate a unique key for the new chat room.
            string chatRoomKey = Guid.NewGuid().ToString("N");

            // Return the key as a JSON object
            return Ok(new { key = chatRoomKey });
        }
    }
}