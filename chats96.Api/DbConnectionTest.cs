using System;
using Microsoft.EntityFrameworkCore;
using chats96.Api.Data;
using Microsoft.Extensions.Configuration;

namespace chats96.Api
{
    public class DbConnectionTest
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Testing database connection...");
            
            try
            {
                // Build configuration
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                    .Build();

                var connectionString = configuration.GetConnectionString("DefaultConnection");
                Console.WriteLine($"Connection String: {connectionString}");

                // Create DbContext options
                var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                optionsBuilder.UseNpgsql(connectionString);

                // Test connection
                using (var context = new ApplicationDbContext(optionsBuilder.Options))
                {
                    Console.WriteLine("Attempting to connect to database...");
                    
                    // Try to open connection
                    context.Database.OpenConnection();
                    Console.WriteLine("✅ Database connection successful!");
                    
                    // Check if database exists and can be created
                    bool canConnect = context.Database.CanConnect();
                    Console.WriteLine($"Can connect to database: {canConnect}");
                    
                    // Check if tables exist
                    var pendingMigrations = context.Database.GetPendingMigrations();
                    Console.WriteLine($"Pending migrations: {pendingMigrations.Count()}");
                    
                    if (pendingMigrations.Any())
                    {
                        Console.WriteLine("Pending migrations found:");
                        foreach (var migration in pendingMigrations)
                        {
                            Console.WriteLine($"  - {migration}");
                        }
                    }
                    
                    context.Database.CloseConnection();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Database connection failed: {ex.Message}");
                Console.WriteLine($"Full error: {ex}");
            }
        }
    }
}
