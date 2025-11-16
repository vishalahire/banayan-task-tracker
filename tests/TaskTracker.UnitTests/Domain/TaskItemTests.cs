using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;

namespace TaskTracker.UnitTests.Domain;

public class TaskItemTests
{
    [Fact]
    public void CreateTaskItem_SetsPropertiesCorrectly()
    {
        // Arrange
        var title = "Test Task";
        var description = "Test Description";
        var ownerId = Guid.NewGuid();
        var dueDate = DateTimeOffset.UtcNow.AddDays(1);
        var priority = TaskPriority.High;

        // Act
        var task = new TaskItem(title, description, ownerId, dueDate, priority);

        // Assert
        Assert.NotEqual(Guid.Empty, task.Id);
        Assert.Equal(title, task.Title);
        Assert.Equal(description, task.Description);
        Assert.Equal(priority, task.Priority);
        Assert.Equal(ownerId, task.OwnerUserId);
        Assert.Equal(dueDate, task.DueDate);
        Assert.Equal(TaskState.New, task.Status);
        Assert.True(task.CreatedAt <= DateTimeOffset.UtcNow);
        Assert.True(task.UpdatedAt <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public void UpdateStatus_UpdatesStatusAndTimestamp()
    {
        // Arrange
        var task = new TaskItem("Test", "Description", Guid.NewGuid());
        var originalUpdatedAt = task.UpdatedAt;
        
        // Wait a small amount to ensure timestamp difference
        Thread.Sleep(1);

        // Act
        task.UpdateStatus(TaskState.InProgress);

        // Assert
        Assert.Equal(TaskState.InProgress, task.Status);
        Assert.True(task.UpdatedAt > originalUpdatedAt);
    }

    [Fact]
    public void MarkComplete_SetsStatusAndCompletedAt()
    {
        // Arrange
        var task = new TaskItem("Test", "Description", Guid.NewGuid());

        // Act
        task.MarkComplete();

        // Assert
        Assert.Equal(TaskState.Completed, task.Status);
        Assert.NotNull(task.CompletedAt);
        Assert.True(task.CompletedAt <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public void IsOwnedBy_WithMatchingUserId_ReturnsTrue()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var task = new TaskItem("Test", "Description", ownerId);

        // Act
        var result = task.IsOwnedBy(ownerId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsOwnedBy_WithDifferentUserId_ReturnsFalse()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();
        var task = new TaskItem("Test", "Description", ownerId);

        // Act
        var result = task.IsOwnedBy(differentUserId);

        // Assert
        Assert.False(result);
    }
}