namespace TaskTracker.Domain.Enums;

public enum AuditAction
{
    TaskCreated = 0,
    TaskUpdated = 1,
    TaskDeleted = 2,
    TaskCompleted = 3,
    AttachmentAdded = 4,
    AttachmentRemoved = 5,
    ReminderSent = 6,
    UserLogin = 7,
    UserProfileUpdated = 8,
    UserCreated = 9
}