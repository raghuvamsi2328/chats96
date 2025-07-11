# Chats96 - Real-time Chat Application

## Database Migration Setup for Portainer Deployment

This project includes automatic database migrations that will run when the Docker container starts.

### How it works:

1. **Migration Files**: Located in `chats96.Api/Migrations/`
   - These files are included in the Git repository
   - They define the database schema (ChatRooms and ChatMessages tables)

2. **Automatic Migration**: The `Program.cs` file includes code that:
   - Checks database connectivity on startup
   - Applies any pending migrations automatically
   - Logs the migration process

3. **Database Tables Created**:
   - `ChatRooms`: Stores chat room information
   - `ChatMessages`: Stores individual messages

### Deployment Process:

1. **Push to Git**: Commit and push changes to your Git repository
2. **Portainer Deployment**: Deploy using Portainer (pulls from Git)
3. **Automatic Setup**: Container starts and applies migrations automatically

### Connection String:

Make sure your docker-compose.yml or Portainer stack includes the correct PostgreSQL configuration:

```yaml
environment:
  - ConnectionStrings__DefaultConnection=Host=chats96-database-1;Port=5432;Username=chatuser;Password=chatpassword;Database=chatdb
```

### Troubleshooting:

- **"relation ChatRooms does not exist"**: Migration didn't run. Check container logs.
- **Connection errors**: Verify database container is running and connection string is correct.
- **Migration errors**: Check the logs in Portainer for detailed error messages.

### Manual Migration (if needed):

If automatic migration fails, you can run migrations manually in the container:

```bash
# Connect to the running API container
docker exec -it <container_name> bash

# Run migrations
dotnet ef database update
```

## Current Status:

✅ Migration files created and ready for deployment
✅ Automatic migration setup in Program.cs
✅ Database models defined (ChatRoom, ChatMessage)
✅ SignalR Hub with error handling
