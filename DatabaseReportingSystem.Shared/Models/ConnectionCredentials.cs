namespace DatabaseReportingSystem.Shared.Models;

public sealed record ConnectionCredentials
{
    public DatabaseManagementSystem DatabaseManagementSystem { get; set; } = DatabaseManagementSystem.Other;

    public string ConnectionHash { get; set; } = string.Empty;
}
