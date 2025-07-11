using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using chats96.Api.Hubs;
using chats96.Api.Data; 
using chats96.Api.Models; 
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq; 
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();




// Configure PostgreSQL DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add SignalR services
builder.Services.AddSignalR();

// Configure CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder.WithOrigins(
            "http://localhost:4200",
            "http://localhost:62499",
            "http://192.168.1.16:4201",
            // Add your deployed frontend URL here when known for testing
            "http://chatapi.server96.com", // Backend API URL
            "https://chatapi.server96.com", // Backend API URL with HTTPS
            "http://chat.server96.com", // Frontend URL
            "https://chat.server96.com", // Frontend URL with HTTPS
            // Add any other frontend domains that might be hosting your Angular app
            "http://localhost:80",
            "https://localhost:443"
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials());
});
builder.WebHost.UseUrls("http://0.0.0.0:80");

var app = builder.Build();


// Apply migrations on startup (for development/testing)
// In production, you might use a separate migration tool or script.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("Checking database connection...");
        
        // Test database connection first
        if (await dbContext.Database.CanConnectAsync())
        {
            logger.LogInformation("Database connection successful.");
            
            // Check if migrations are needed
            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                logger.LogInformation($"Applying {pendingMigrations.Count()} pending migrations...");
                await dbContext.Database.MigrateAsync();
                logger.LogInformation("Database migrations applied successfully.");
            }
            else
            {
                logger.LogInformation("Database is up to date. No migrations needed.");
            }
        }
        else
        {
            logger.LogError("Cannot connect to database. Please check connection string and ensure database server is running.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error during database migration: {Message}", ex.Message);
        // Don't exit the application, let it start anyway for debugging
        logger.LogWarning("Application will continue to start despite database migration errors.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// For production, if you are serving from an ingress controller that handles HTTPS,
// you might not need app.UseHttpsRedirection() here.
// app.UseHttpsRedirection();
app.MapHealthChecks("/health");
app.UseWebSockets();
app.UseRouting();
app.UseCors("AllowSpecificOrigin");
app.UseAuthorization();
app.MapControllers();
app.MapHub<ChatsHub>("/chatsHub");

app.Run();