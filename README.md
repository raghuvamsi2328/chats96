# Chats96 - Real-time Chat Application

A modern, real-time chat application built with ASP.NET Core, Angular, and SignalR. Create instant chat rooms, share links, and communicate in real-time with automatic message persistence.

## 🚀 Features

- **Real-time Messaging**: Instant message delivery using SignalR WebSockets
- **Room Sharing**: Generate unique room links to invite others
- **Message Persistence**: All messages are saved to PostgreSQL database
- **User Tracking**: See who's currently in each room
- **Theme Support**: Light and dark theme toggle
- **Auto-cleanup**: Empty rooms are automatically deleted
- **Historical Messages**: Previous messages load when joining a room
- **Responsive Design**: Works on desktop and mobile devices
- **Docker Ready**: Containerized deployment with docker-compose

## 🛠️ Tech Stack

### Backend
- **ASP.NET Core 8.0** - Web API framework
- **SignalR** - Real-time communication
- **Entity Framework Core** - ORM for database operations
- **PostgreSQL** - Database for message persistence
- **Docker** - Containerization

### Frontend
- **Angular 17** - Frontend framework
- **TypeScript** - Type-safe JavaScript
- **SCSS** - Styling with variables and themes
- **Microsoft SignalR Client** - Real-time connection to backend

## 📋 Prerequisites

- **Docker & Docker Compose** (recommended)
- **OR Manual Setup**:
  - .NET 8.0 SDK
  - Node.js 18+ and npm
  - PostgreSQL 15+

## 🐳 Quick Start with Docker

1. **Clone the repository**
   ```bash
   git clone <your-repo-url>
   cd chats96
   ```

2. **Start the application**
   ```bash
   docker-compose up -d
   ```

3. **Access the application**
   - Frontend: http://localhost:4201
   - Backend API: http://localhost:5001
   - Database: localhost:5433

4. **Create or join a chat room**
   - Enter your name
   - Click "Create New Chat" or join with a shared link

## 🔧 Manual Development Setup

### Backend Setup

1. **Navigate to API directory**
   ```bash
   cd chats96.Api
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Update connection string** in `appsettings.Development.json`
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5432;Username=your_user;Password=your_password;Database=chatdb"
     }
   }
   ```

4. **Run database migrations**
   ```bash
   dotnet ef database update
   ```

5. **Start the API**
   ```bash
   dotnet run
   ```

### Frontend Setup

1. **Navigate to Angular app directory**
   ```bash
   cd chats96App
   ```

2. **Install dependencies**
   ```bash
   npm install
   ```

3. **Update backend URL** in `src/environments/environment.development.ts`
   ```typescript
   export const environment = {
     production: false,
     backendUrl: 'https://localhost:7001' // Your API URL
   };
   ```

4. **Start the development server**
   ```bash
   ng serve
   ```

5. **Open browser**
   - Navigate to http://localhost:4200

## 📱 How to Use

### Creating a Chat Room
1. Enter your display name
2. Click "Create New Chat"
3. Share the generated URL with others

### Joining a Chat Room
1. Click on a shared chat room link
2. Enter your display name
3. Click "Join Chat"

### Sharing a Room
- Click the "Share Room" button to copy the room link
- Send the link to anyone you want to invite

## 🏗️ Project Structure

```
chats96/
├── chats96.Api/                 # ASP.NET Core Web API
│   ├── Controllers/             # REST API controllers
│   ├── Data/                    # Entity Framework DbContext
│   ├── Hubs/                    # SignalR hubs
│   ├── Migrations/              # Database migrations
│   ├── Models/                  # Data models
│   └── Program.cs               # Application entry point
├── chats96App/                  # Angular frontend
│   ├── src/
│   │   ├── app/
│   │   │   ├── chat-window/     # Chat interface component
│   │   │   ├── app.component.*  # Main app component
│   │   │   └── ...
│   │   └── environments/        # Environment configurations
│   └── ...
├── docker-compose.yml           # Docker orchestration
├── docker-compose.alt-ports.yml # Alternative port configuration
└── DATABASE_SETUP.md           # Database setup guide
```

## 🗄️ Database Schema

### ChatRooms Table
- `ChatRoomKey` (PK): Unique room identifier
- `CreatedAt`: Room creation timestamp
- `ActiveUsers`: Current user count
- `LastActivity`: Last activity timestamp

### ChatMessages Table
- `Id` (PK): Message identifier
- `Sender`: User display name
- `MessageContent`: Message text
- `Timestamp`: When message was sent
- `ChatRoomKey` (FK): Reference to chat room

## 🔌 API Endpoints

### REST API
- `POST /api/chat/create` - Create new chat room
- `GET /health` - Health check endpoint

### SignalR Hub (`/chatsHub`)
- `JoinChatRoom(roomKey, userName)` - Join a chat room
- `SendMessage(roomKey, message)` - Send a message
- `ReceiveMessage` - Receive messages (client event)
- `UserJoined` - User join notification (client event)

## 🐋 Docker Configuration

### Services
- **backend**: ASP.NET Core API (port 5001)
- **frontend**: Angular app served by Nginx (port 4201)
- **database**: PostgreSQL 15 (port 5433)

### Environment Variables
```env
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Host=database;Port=5432;Username=chatuser;Password=chatpassword;Database=chatdb
POSTGRES_DB=chatdb
POSTGRES_USER=chatuser
POSTGRES_PASSWORD=chatpassword
```

## 🔍 Troubleshooting

### Common Issues

1. **Port Conflicts**
   - Backend: Change port in `docker-compose.yml`
   - Frontend: Modify `ports` section
   - Database: Use alternative port (5433 is configured)

2. **Database Connection Issues**
   - Check if PostgreSQL container is running
   - Verify connection string in `appsettings.json`
   - Check logs: `docker-compose logs backend`

3. **Migration Errors**
   - Automatic migrations run on startup
   - Check container logs for migration status
   - Manual migration: `docker exec -it <container> dotnet ef database update`

4. **SignalR Connection Issues**
   - Verify CORS settings in `Program.cs`
   - Check browser console for connection errors
   - Ensure backend URL is correct in Angular environment files

### Viewing Logs
```bash
# View all service logs
docker-compose logs

# View specific service logs
docker-compose logs backend
docker-compose logs frontend
docker-compose logs database
```

## 🚀 Deployment

### Production Deployment
1. Update environment URLs in Angular environment files
2. Set production connection strings
3. Use `docker-compose up -d` for detached mode
4. Configure reverse proxy (Nginx/Apache) if needed

### Portainer Deployment
1. Create new stack in Portainer
2. Copy `docker-compose.yml` content
3. Set environment variables
4. Deploy stack

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License.

## 🆘 Support

For issues and questions:
1. Check the troubleshooting section
2. Review container logs
3. Open an issue in the repository

---

**Built with ❤️ using ASP.NET Core, Angular, and SignalR**