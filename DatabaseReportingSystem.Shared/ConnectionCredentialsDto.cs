namespace DatabaseReportingSystem.Shared;

public sealed record ConnectionCredentialsDto(
    string Dbms,
    string Host,
    string Port,
    string Database,
    string Schema,
    string Password);
