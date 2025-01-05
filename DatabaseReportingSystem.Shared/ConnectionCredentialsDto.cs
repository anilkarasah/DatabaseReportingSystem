using DatabaseReportingSystem.Shared.Models;

namespace DatabaseReportingSystem.Shared;

public sealed record ConnectionCredentialsDto(
    DatabaseManagementSystem Dbms,
    string Host,
    string Port,
    string Database,
    string Schema,
    string Password);
