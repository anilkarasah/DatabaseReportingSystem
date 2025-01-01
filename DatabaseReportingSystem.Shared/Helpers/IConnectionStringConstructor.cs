namespace DatabaseReportingSystem.Shared.Helpers;

public interface IConnectionStringConstructor
{
    string Generate(string host, string port, string database, string schema, string username, string password);
}

public sealed class PostgreSqlConnectionStringConstructor : IConnectionStringConstructor
{
    public string Generate(string host, string port, string database, string schema, string username, string password)
    {
        return $"Host={host};Port={port};Database={database};Username={username};Password={password};";
    }
}

public sealed class MySqlConnectionStringConstructor : IConnectionStringConstructor
{
    public string Generate(string host, string port, string database, string schema, string username, string password)
    {
        return $"Server={host};Port={port};Database={database};User={username};Password={password};";
    }
}

public sealed class SqlServerConnectionStringConstructor : IConnectionStringConstructor
{
    public string Generate(string host, string port, string database, string schema, string username, string password)
    {
        return $"Server={host},{port};Database={database};User Id={username};Password={password};";
    }
}
