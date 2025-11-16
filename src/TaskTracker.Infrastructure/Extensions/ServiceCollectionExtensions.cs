using Microsoft.Extensions.DependencyInjection;
using TaskTracker.Application.Interfaces;
using TaskTracker.Application.Repositories;
using TaskTracker.Application.Services;
using TaskTracker.Infrastructure.Repositories;
using TaskTracker.Infrastructure.Services;

namespace TaskTracker.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTaskTrackerApplicationServices(this IServiceCollection services)
    {
        // Register repositories
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IAttachmentRepository, AttachmentRepository>();
        services.AddScoped<IAuditRepository, AuditRepository>();
        services.AddScoped<IReminderLogRepository, ReminderLogRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        // Register application services
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<IAttachmentService, AttachmentService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IReminderService, ReminderService>();
        services.AddScoped<IAuthService, AuthService>();

        // Register infrastructure services
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        return services;
    }
}