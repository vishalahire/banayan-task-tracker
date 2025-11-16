using Microsoft.EntityFrameworkCore;
using Serilog;
using TaskTracker.Infrastructure.Data;
using TaskTracker.Infrastructure.Extensions;
using TaskTracker.Worker.Services;

var builder = Host.CreateApplicationBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Services.AddSerilog();

// Configure Entity Framework
// Try environment variable first, then fall back to configuration
var connectionString = Environment.GetEnvironmentVariable("TASKTRACKER_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("TaskTracker")
    ?? throw new InvalidOperationException("Connection string not found. Set TASKTRACKER_CONNECTION_STRING environment variable or configure 'TaskTracker' connection string.");

builder.Services.AddDbContext<TaskTrackerDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register application services (shared with API)
builder.Services.AddTaskTrackerApplicationServices();

// Register the background service
builder.Services.AddHostedService<ReminderProcessingService>();

var host = builder.Build();

// Ensure database exists (for development)
using (var scope = host.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TaskTrackerDbContext>();
    try
    {
        context.Database.EnsureCreated();
        Log.Information("Database connection verified");
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "Failed to connect to database");
        return;
    }
}

try
{
    Log.Information("Starting TaskTracker Worker");
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Worker terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}