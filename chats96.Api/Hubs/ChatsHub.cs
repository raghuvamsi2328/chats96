using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System;
using chats96.Api.Data; // Import your DbContext
using chats96.Api.Models; // Import your models
using Microsoft.EntityFrameworkCore; // For Include and FirstOrDefaultAsync
using Microsoft.Extensions.DependencyInjection; // For IServiceScopeFactory
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent; // For logging

namespace chats96.Api.Hubs
{
    public class ChatsHub : Hub
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ChatsHub> _logger;
        // In-memory tracking of active connections per room (for temporary user count)
        private static readonly ConcurrentDictionary<string, int> _activeUserCounts = new ConcurrentDictionary<string, int>();

        public ChatsHub(IServiceScopeFactory scopeFactory, ILogger<ChatsHub> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        // Method for clients to join a specific chat room group
        public async Task JoinChatRoom(string chatRoomKey, string chatName)
        {
            try
            {
                // Validate input parameters
                if (string.IsNullOrWhiteSpace(chatRoomKey))
                {
                    _logger.LogError($"[SignalR] Invalid chatRoomKey provided by connection {Context.ConnectionId}");
                    await Clients.Caller.SendAsync("Error", "Invalid chat room key");
                    return;
                }

                if (string.IsNullOrWhiteSpace(chatName))
                {
                    _logger.LogError($"[SignalR] Invalid chatName provided by connection {Context.ConnectionId}");
                    await Clients.Caller.SendAsync("Error", "Invalid chat name");
                    return;
                }

                _logger.LogInformation($"[SignalR] Connection {Context.ConnectionId} ({chatName}) attempting to join room: {chatRoomKey}");

                // Track this connection to the room for cleanup on disconnect
                UserConnectionTracker.ConnectionToRoomMap.TryAdd(Context.ConnectionId, chatRoomKey);

                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    // 1. Get or Create ChatRoom in DB
                    var chatRoom = await dbContext.ChatRooms
                                                 .FirstOrDefaultAsync(cr => cr.ChatRoomKey == chatRoomKey);

                    if (chatRoom == null)
                    {
                        chatRoom = new ChatRoom { ChatRoomKey = chatRoomKey };
                        dbContext.ChatRooms.Add(chatRoom);
                        await dbContext.SaveChangesAsync();
                        _logger.LogInformation($"[SignalR] Created new chat room in DB: {chatRoomKey}");
                    }

                    // 2. Increment active user count (in-memory for real-time tracking)
                    _activeUserCounts.AddOrUpdate(chatRoomKey, 1, (key, count) => count + 1);

                    // Update DB count (more persistent tracking, though in-memory is faster for hub)
                    chatRoom.ActiveUsers = _activeUserCounts[chatRoomKey];
                    chatRoom.LastActivity = DateTime.UtcNow; // Update last activity
                    await dbContext.SaveChangesAsync();

                    // 3. Add connection to SignalR group
                    await Groups.AddToGroupAsync(Context.ConnectionId, chatRoomKey);

                    // 4. Notify everyone in the group
                    await Clients.Group(chatRoomKey).SendAsync("UserJoined", chatName, chatRoomKey, chatRoom.ActiveUsers);
                    _logger.LogInformation($"[SignalR] Connection {Context.ConnectionId} ({chatName}) joined room: {chatRoomKey}. Active users: {chatRoom.ActiveUsers}");

                    // 5. Send historical messages to the newly joined user
                    var historicalMessages = await dbContext.ChatMessages
                                                            .Where(cm => cm.ChatRoomKey == chatRoomKey)
                                                            .OrderBy(cm => cm.Timestamp)
                                                            .Select(cm => new ChatMessage // Project to ChatMessage type used by client
                                                            {
                                                                Sender = cm.Sender,
                                                                MessageContent = cm.MessageContent, // Use MessageContent
                                                                Timestamp = cm.Timestamp,
                                                                ChatRoomKey = cm.ChatRoomKey
                                                            })
                                                            .ToListAsync();
                    if (historicalMessages.Count > 0)
                    {
                        await Clients.Caller.SendAsync("LoadHistoricalMessages", historicalMessages);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[SignalR] Error in JoinChatRoom for connection {Context.ConnectionId}");
                await Clients.Caller.SendAsync("Error", "Failed to join chat room");
                
                // Clean up connection tracking on error
                UserConnectionTracker.ConnectionToRoomMap.TryRemove(Context.ConnectionId, out _);
            }
        }

        // Method for clients to send messages
        public async Task SendMessage(string chatRoomKey, ChatMessage clientMessage)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(chatRoomKey))
                {
                    _logger.LogError($"[SignalR] Invalid chatRoomKey in SendMessage from connection {Context.ConnectionId}");
                    await Clients.Caller.SendAsync("Error", "Invalid chat room key");
                    return;
                }

                if (clientMessage == null || string.IsNullOrWhiteSpace(clientMessage.MessageContent))
                {
                    _logger.LogError($"[SignalR] Empty message content from connection {Context.ConnectionId}");
                    return;
                }

                if (string.IsNullOrWhiteSpace(clientMessage.Sender))
                {
                    _logger.LogError($"[SignalR] Invalid sender in SendMessage from connection {Context.ConnectionId}");
                    await Clients.Caller.SendAsync("Error", "Invalid sender");
                    return;
                }

                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    // 1. Create message entity from client data
                    var messageToSave = new ChatMessage
                    {
                        ChatRoomKey = chatRoomKey,
                        Sender = clientMessage.Sender,
                        MessageContent = clientMessage.MessageContent, // Use MessageContent
                        Timestamp = DateTime.UtcNow // Always set server-side
                    };

                    // 2. Save message to DB
                    dbContext.ChatMessages.Add(messageToSave);

                    // 3. Update chat room's last activity
                    var chatRoom = await dbContext.ChatRooms.FirstOrDefaultAsync(cr => cr.ChatRoomKey == chatRoomKey);
                    if (chatRoom != null)
                    {
                        chatRoom.LastActivity = DateTime.UtcNow;
                    }
                    await dbContext.SaveChangesAsync();
                    _logger.LogInformation($"[SignalR] Received & saved message from {messageToSave.Sender} in room {chatRoomKey}: {messageToSave.MessageContent}");

                    // 4. Broadcast the message to all clients in the same group
                    // Re-project the message to ensure it matches the client's ChatMessage interface for consistency
                    var messageToClient = new ChatMessage
                    {
                        Sender = messageToSave.Sender,
                        MessageContent = messageToSave.MessageContent,
                        Timestamp = messageToSave.Timestamp,
                        ChatRoomKey = messageToSave.ChatRoomKey
                    };
                    await Clients.Group(chatRoomKey).SendAsync("ReceiveMessage", messageToClient);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[SignalR] Error in SendMessage for connection {Context.ConnectionId}");
                await Clients.Caller.SendAsync("Error", "Failed to send message");
            }
        }

        // Handle when a client disconnects
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                _logger.LogInformation($"[SignalR] Connection {Context.ConnectionId} disconnected. Exception: {exception?.Message}");

                // We need to know which room this connection was in to decrement its count.
                // SignalR doesn't automatically tell us the group on disconnect.
                // A more robust solution would involve tracking connections to rooms in a ConcurrentDictionary
                // or by looking up Context.User if using authentication.
                // For simplicity in this example, we'll try to find any rooms this connection was in.

                // This is a basic approach and might not be perfect for every scenario
                // A better way for tracking active users is to use a custom IUserIdProvider or a service
                // that maps ConnectionId to ChatRoomKey.

                // SignalR 2.x and later remove connection-group mappings on disconnect automatically.
                // We just need to update our user count.

                // This part is tricky without knowing which room the client was in easily.
                // A common pattern is to pass chatRoomKey into the Context.Items on connect,
                // or to have a separate service manage ConnectionId -> RoomId mappings.

                // For a basic setup, let's assume `Context.Items` has the room key set by `JoinChatRoom`
                // (Note: Context.Items is volatile across different hub instances, a shared ConcurrentDictionary is better)
                // Let's implement a more robust way to track active users per room for cleanup
                await DecrementActiveUsersAndCleanup();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[SignalR] Error in OnDisconnectedAsync for connection {Context.ConnectionId}");
            }
            finally
            {
                await base.OnDisconnectedAsync(exception);
            }
        }

        // --- Helper for cleanup ---
        private async Task DecrementActiveUsersAndCleanup()
        {
            try
            {
                // The problem: OnDisconnectedAsync doesn't easily tell us which group the user was in.
                // We need a way to track ConnectionId to ChatRoomKey.
                // Let's create a simple in-memory map for this.
                // Note: In a load-balanced environment, this map would need to be in a distributed cache (e.g., Redis).

                string? chatRoomKey = null;

                // Retrieve the chatRoomKey that was associated with this connection when it joined.
                // This requires storing it somewhere accessible. Let's use a new ConcurrentDictionary.
                if (UserConnectionTracker.ConnectionToRoomMap.TryRemove(Context.ConnectionId, out var roomKeyFromMap))
                {
                    chatRoomKey = roomKeyFromMap;
                }

                if (chatRoomKey != null)
                {
                    _logger.LogInformation($"[SignalR] User from room {chatRoomKey} disconnected.");

                    // Decrement active user count
                    _activeUserCounts.AddOrUpdate(chatRoomKey, 0, (key, count) => Math.Max(0, count - 1));

                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        var chatRoom = await dbContext.ChatRooms.FirstOrDefaultAsync(cr => cr.ChatRoomKey == chatRoomKey);

                        if (chatRoom != null)
                        {
                            chatRoom.ActiveUsers = _activeUserCounts.GetValueOrDefault(chatRoomKey);
                            await dbContext.SaveChangesAsync();
                            _logger.LogInformation($"[SignalR] Room {chatRoomKey} active users updated to: {chatRoom.ActiveUsers}");

                            // Cleanup logic: If active users become 0, schedule deletion
                            if (chatRoom.ActiveUsers <= 0)
                            {
                                _logger.LogInformation($"[SignalR] Room {chatRoomKey} has 0 active users. Scheduling for deletion.");
                                // Instead of immediate deletion, which might be risky with temporary disconnects,
                                // we'll use a timer or a background service to clean up after a delay.
                                // For this example, let's just delete it immediately for demonstration.
                                // In a real app: Use a background service that periodically checks LastActivity + a grace period.

                                // Immediate deletion for demo purposes (NOT FOR PROD WITHOUT GRACE PERIOD!)
                                var messagesToDelete = await dbContext.ChatMessages
                                                                    .Where(cm => cm.ChatRoomKey == chatRoomKey)
                                                                    .ToListAsync();
                                dbContext.ChatMessages.RemoveRange(messagesToDelete);
                                dbContext.ChatRooms.Remove(chatRoom);
                                await dbContext.SaveChangesAsync();
                                _logger.LogInformation($"[SignalR] Room {chatRoomKey} and its messages deleted due to no active users.");
                            }
                        }
                    }
                }
                else
                {
                    _logger.LogWarning($"[SignalR] Could not find room mapping for disconnected connection {Context.ConnectionId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[SignalR] Error in DecrementActiveUsersAndCleanup for connection {Context.ConnectionId}");
            }
        }
    }

    // A simple in-memory static class to map ConnectionId to ChatRoomKey
    // Note: In a multi-instance deployment, this would need to be a distributed cache (e.g., Redis)
    public static class UserConnectionTracker
    {
        public static ConcurrentDictionary<string, string> ConnectionToRoomMap = new ConcurrentDictionary<string, string>();
    }
}