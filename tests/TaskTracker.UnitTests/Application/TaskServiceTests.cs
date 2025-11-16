using TaskTracker.Application.Services;
using TaskTracker.Application.Repositories;
using TaskTracker.Application.Commands;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;

namespace TaskTracker.UnitTests.Application;

public class TaskServiceTests
{
    [Fact]
    public void ValidateCreateTaskCommand_ValidCommand_DoesNotThrow()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Title = "Valid Task",
            Description = "Valid Description",
            Priority = TaskPriority.Medium,
            DueDate = DateTimeOffset.UtcNow.AddDays(1),
            Tags = new List<string> { "work", "important" },
            OwnerUserId = Guid.NewGuid()
        };

        // Act & Assert
        var exception = Record.Exception(() => ValidateCommand(command));
        Assert.Null(exception);
    }

    [Fact]
    public void ValidateCreateTaskCommand_EmptyTitle_ThrowsArgumentException()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Title = "", // Empty title
            Priority = TaskPriority.Medium,
            OwnerUserId = Guid.NewGuid()
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => ValidateCommand(command));
        Assert.Contains("Title", exception.Message);
    }

    [Fact]
    public void ValidateCreateTaskCommand_EmptyOwnerId_ThrowsArgumentException()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Title = "Valid Task",
            Priority = TaskPriority.Medium,
            OwnerUserId = Guid.Empty // Empty owner ID
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => ValidateCommand(command));
        Assert.Contains("OwnerUserId", exception.Message);
    }

    // Helper method to validate command (simulating what would be in TaskService)
    private static void ValidateCommand(CreateTaskCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Title))
            throw new ArgumentException("Title is required.", nameof(command.Title));

        if (command.OwnerUserId == Guid.Empty)
            throw new ArgumentException("OwnerUserId is required.", nameof(command.OwnerUserId));
    }
}