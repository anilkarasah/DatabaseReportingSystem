using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using DatabaseReportingSystem.Shared.Helpers;
using DatabaseReportingSystem.Shared.Models;
using MySql.Data.MySqlClient;
using Npgsql;
using OpenAI.Chat;

namespace DatabaseReportingSystem.Shared;

public static class Utilities
{
    public static string TrimSqlString(string query)
    {
        string[] parts = query.Split(["```sql", "```"], StringSplitOptions.None);

        if (parts.Length > 1)
        {
            query = parts[1];
        }

        query = query.Replace("\n", " ").Replace("\t", "");

        query = string.Join(" ", query.Split([' '], StringSplitOptions.RemoveEmptyEntries));

        return query.Trim();
    }

    public static UserChatMessage CreateUserChatMessage(string question, string schema, DatabaseManagementSystem dbms)
    {
        string message = string.Format(Constants.Strategy.UserPromptFormat,
            question,
            dbms.ToString().ToLower(),
            schema);

        return new UserChatMessage(message);
    }

    public static DatabaseManagementSystem AsDatabaseManagementSystem(this string dbms)
        => GetDatabaseManagementSystem(dbms);

    public static DatabaseManagementSystem GetDatabaseManagementSystem(string dbms)
    {
        return dbms.ToLower() switch
        {
            "sqlite" => DatabaseManagementSystem.Sqlite,
            "sqlserver" => DatabaseManagementSystem.SqlServer,
            "mssql" => DatabaseManagementSystem.SqlServer,
            "mysql" => DatabaseManagementSystem.MySql,
            "pgsql" => DatabaseManagementSystem.PostgreSql,
            "npgsql" => DatabaseManagementSystem.PostgreSql,
            "postgresql" => DatabaseManagementSystem.PostgreSql,
            _ => DatabaseManagementSystem.Other,
        };
    }

    public static LargeLanguageModel AsLargeLanguageModel(this string modelName)
        => GetLargeLanguageModel(modelName);

    public static LargeLanguageModel GetLargeLanguageModel(string modelName)
        => modelName switch
        {
            "gpt-4o-mini" => LargeLanguageModel.GPT,
            "gpt-4o" => LargeLanguageModel.GPT,
            "gpt" => LargeLanguageModel.GPT,
            "grok-beta" => LargeLanguageModel.Grok,
            "grok" => LargeLanguageModel.Grok,
            "mistral" => LargeLanguageModel.Mistral,
            "codellama" => LargeLanguageModel.CodeLLaMa,
            "llama" => LargeLanguageModel.CodeLLaMa,
            _ => throw new ArgumentException("Invalid model name.")
        };

    public static StrategyType AsStrategyType(this string strategyType)
        => GetStrategyType(strategyType);

    public static StrategyType GetStrategyType(string strategyName)
        => strategyName switch
        {
            "basic" => StrategyType.ZeroShot,
            "zero" => StrategyType.ZeroShot,
            "zero-shot" => StrategyType.ZeroShot,
            "random" => StrategyType.RandomFewShot,
            "random-shot" => StrategyType.RandomFewShot,
            "random-few-shot" => StrategyType.RandomFewShot,
            "nearest" => StrategyType.NearestFewShot,
            "nearest-shot" => StrategyType.NearestFewShot,
            "nearest-few-shot" => StrategyType.NearestFewShot,
            _ => throw new ArgumentException("Invalid strategy name.")
        };

    public static string GenerateConnectionString(
        DatabaseManagementSystem databaseManagementSystem,
        ConnectionCredentialsDto connectionCredentials)
    {
        IConnectionStringConstructor connectionStringConstructor = databaseManagementSystem switch
        {
            DatabaseManagementSystem.SqlServer => new SqlServerConnectionStringConstructor(),
            DatabaseManagementSystem.MySql => new MySqlConnectionStringConstructor(),
            DatabaseManagementSystem.PostgreSql => new PostgreSqlConnectionStringConstructor(),
            DatabaseManagementSystem.Sqlite => throw new ApplicationException("SQLite is not supported."),
            _ => throw new ArgumentException("Invalid database management system.")
        };

        return connectionStringConstructor.Generate(
            connectionCredentials.Host,
            connectionCredentials.Port,
            connectionCredentials.Database,
            connectionCredentials.Schema,
            Constants.DefaultDatabaseUsername,
            connectionCredentials.Password);
    }

    internal static DbConnection GetDatabaseConnection(
        DatabaseManagementSystem databaseManagementSystem,
        string connectionString)
    {
        DbConnection connection = databaseManagementSystem switch
        {
            DatabaseManagementSystem.SqlServer => new SqlConnection(connectionString),
            DatabaseManagementSystem.MySql => new MySqlConnection(connectionString),
            DatabaseManagementSystem.PostgreSql => new NpgsqlConnection(connectionString),
            DatabaseManagementSystem.Sqlite => new System.Data.SQLite.SQLiteConnection(connectionString),
            _ => throw new ArgumentException("Database management system does not support connection tests."),
        };

        return connection;
    }

    public static Result TestDatabaseConnection(
        DatabaseManagementSystem databaseManagementSystem,
        string connectionString)
    {
        try
        {
            using DbConnection connection = GetDatabaseConnection(databaseManagementSystem, connectionString);

            connection.Open();

            using DbCommand command = connection.CreateCommand();

            command.CommandText = "SELECT 1";
            command.CommandType = CommandType.Text;

            object? result = command.ExecuteScalar();

            if (result is 1)
            {
                return Result.Ok();
            }
        }
        catch (Exception e)
        {
            return Result.Fail(e.Message);
        }
        
        return Result.Fail("Connection test failed.");
    }
}
