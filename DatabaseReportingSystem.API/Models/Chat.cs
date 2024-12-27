namespace DatabaseReportingSystem.Models;

public sealed class Chat
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string DatabaseManagementSystem { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public User User { get; set; } = null!;

    public List<ChatMessage> Messages { get; set; } = [];
}
