namespace DatabaseReportingSystem.Shared.Models;

public sealed record ConnectionCredentials
{
    public string DatabaseManagementSystem { get; set; } = string.Empty;

    public string ConnectionHash { get; set; } = string.Empty;
}
