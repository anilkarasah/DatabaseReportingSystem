namespace DatabaseReportingSystem.Models;

public record ConnectionCredentials
{
    public string DatabaseManagementSystem { get; set; } = string.Empty;

    public string ConnectionHash { get; set; } = string.Empty;
}
