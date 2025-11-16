namespace TaskTracker.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public User()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public User(string email, string displayName) : this()
    {
        Email = email;
        DisplayName = displayName;
    }

    public void UpdateProfile(string displayName)
    {
        DisplayName = displayName;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}