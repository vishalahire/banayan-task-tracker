using Microsoft.AspNetCore.Authorization;

namespace TaskTracker.Api.Authorization;

public static class Policies
{
    public const string TaskOwner = "TaskOwner";
}

public class TaskOwnerRequirement : IAuthorizationRequirement
{
    public Guid TaskId { get; }

    public TaskOwnerRequirement(Guid taskId)
    {
        TaskId = taskId;
    }
}

public class TaskOwnerAuthorizationHandler : AuthorizationHandler<TaskOwnerRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, TaskOwnerRequirement requirement)
    {
        // TODO: Implement task ownership validation logic
        // This will be implemented when we create the controllers
        // For now, we'll mark it as succeeded to allow development
        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}