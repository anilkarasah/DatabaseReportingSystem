namespace DatabaseReportingSystem.Models;

public sealed class User
{
    public Guid Id { get; set; }

    public string Email { get; set; }

    public string PasswordHash { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public Status Status { get; set; } = Status.Draft;

    public ConnectionCredentials ConnectionCredentials { get; set; } = null!;

    public List<UserLicense> Licenses { get; set; } = [];

    public List<Chat> Chats { get; set; } = [];
}
