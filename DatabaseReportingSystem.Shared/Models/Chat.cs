namespace DatabaseReportingSystem.Shared.Models;

public sealed class Chat
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public DatabaseManagementSystem DatabaseManagementSystem { get; set; } = DatabaseManagementSystem.Other;

    public string SchemaHash { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public User User { get; set; } = null!;

    public List<ChatMessage> Messages { get; set; } = [];
}
