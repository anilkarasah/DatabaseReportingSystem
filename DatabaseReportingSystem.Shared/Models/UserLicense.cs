namespace DatabaseReportingSystem.Shared.Models;

public sealed class UserLicense
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string LicenseCode { get; set; } = string.Empty;

    public DateTimeOffset StartsAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset ExpiresAtUtc { get; set; } = DateTimeOffset.UtcNow.AddMonths(1);

    public Status Status { get; set; } = Status.Draft;

    public User User { get; set; } = null!;
}
