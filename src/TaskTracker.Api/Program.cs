using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using System.Text.Json;

using TaskTracker.Api.Authorization;
using TaskTracker.Api.Middleware;
using TaskTracker.Infrastructure.Data;
using TaskTracker.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

// Configure Entity Framework
// Try environment variable first, then fall back to configuration
var connectionString = Environment.GetEnvironmentVariable("TASKTRACKER_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("TaskTracker")
    ?? throw new InvalidOperationException("Connection string not found. Set TASKTRACKER_CONNECTION_STRING environment variable or configure 'TaskTracker' connection string.");

builder.Services.AddDbContext<TaskTrackerDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register application services
builder.Services.AddTaskTrackerApplicationServices();

// Configure ASP.NET Core Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    
    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    
    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<TaskTrackerDbContext>()
.AddDefaultTokenProviders();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
var key = Encoding.ASCII.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Set to true in production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Configure Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.TaskOwner, policy =>
        policy.Requirements.Add(new TaskOwnerRequirement(Guid.Empty)));
});

builder.Services.AddScoped<IAuthorizationHandler, TaskOwnerAuthorizationHandler>();

// Rate limiting removed due to compatibility issues with .NET 8

// Configure Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<TaskTrackerDbContext>(name: "database")
    .AddNpgSql(connectionString, name: "postgresql");

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("TaskTrackerPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173") // React dev servers
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "TaskTracker API",
        Description = "A task management API with authentication and file attachments",
    });

    // Configure JWT authentication in Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskTracker API v1");
        options.RoutePrefix = string.Empty; // Serve Swagger UI at the root
    });
}

// Custom middleware pipeline
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseCors("TaskTrackerPolicy");



app.UseAuthentication();
app.UseAuthorization();

// Health check endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new()
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapControllers();

// Ensure database and Identity tables are created
Log.Information("=== DATABASE CREATION BLOCK REACHED ===");
using (var scope = app.Services.CreateScope())
{
    Log.Information("=== SCOPE CREATED ===");
    var context = scope.ServiceProvider.GetRequiredService<TaskTrackerDbContext>();
    Log.Information("=== CONTEXT RETRIEVED ===");
    
    try
    {
        Log.Information("Attempting to ensure database is created...");
        var created = context.Database.EnsureCreated();
        Log.Information("Database ensure created result: {Created}", created);
        
        Log.Information("Database creation completed successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error ensuring database schema");
        throw;
    }
}

try
{
    Log.Information("Starting TaskTracker API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}