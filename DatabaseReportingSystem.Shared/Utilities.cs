﻿using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Diagnostics;
using Antlr4.Runtime;
using DatabaseReportingSystem.Shared.Antlr;
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
        try
        {
            if (string.IsNullOrWhiteSpace(query)) return string.Empty;

            // Remove code block indicator
            if (query.Contains("```sql"))
            {
                query = query.Split("```sql", StringSplitOptions.RemoveEmptyEntries).Last();
            }
            else if (query.Contains("```"))
            {
                query = query.Split("```", StringSplitOptions.RemoveEmptyEntries).Last();
            }

            query = query.Split("```", StringSplitOptions.RemoveEmptyEntries).First();

            // Remove recurring spaces
            query = string.Join(" ", query.Split([' '], StringSplitOptions.RemoveEmptyEntries));

            // Trim whitespace from the beginning and end
            query = query
                .Trim('\n')
                .Trim('\t');

            return query.Trim();
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    public static string CreateUserMessage(UserPromptDto dto)
    {
        return string.Format(Constants.Strategy.UserPromptFormat,
            dto.Question,
            dto.DatabaseManagementSystem.ToString().ToLower(),
            dto.Schema);
    }

    public static UserChatMessage CreateUserChatMessage(UserPromptDto dto)
    {
        string message = CreateUserMessage(dto);
        return new UserChatMessage(message);
    }

    public static LargeLanguageModel AsLargeLanguageModel(this string modelName)
    {
        if (Enum.TryParse(modelName, out LargeLanguageModel model)) return model;

        return modelName switch
        {
            "gpt-4o-mini" => LargeLanguageModel.GPT,
            "gpt" => LargeLanguageModel.GPT,
            "grok-beta" => LargeLanguageModel.Grok,
            "grok" => LargeLanguageModel.Grok,
            "mistral" => LargeLanguageModel.Mistral,
            "codellama" => LargeLanguageModel.CodeLLaMa,
            "llama" => LargeLanguageModel.CodeLLaMa,
            _ => throw new ArgumentException("Invalid model name.")
        };
    }

    public static StrategyType AsStrategyType(this string strategyType)
    {
        if (Enum.TryParse(strategyType, out StrategyType model)) return model;

        return strategyType switch
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
    }

    public static string GenerateConnectionString(ConnectionCredentialsDto connectionCredentials)
    {
        IConnectionStringConstructor connectionStringConstructor = connectionCredentials.Dbms switch
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
            DatabaseManagementSystem.Sqlite => new SQLiteConnection(connectionString),
            _ => throw new ArgumentException("Database management system does not support connection tests.")
        };

        return connection;
    }

    public static async Task<Result> TestDatabaseConnectionAsync(
        DatabaseManagementSystem databaseManagementSystem,
        string connectionString)
    {
        try
        {
            await using DbConnection connection = GetDatabaseConnection(databaseManagementSystem, connectionString);

            await connection.OpenAsync();

            await using DbCommand command = connection.CreateCommand();

            command.CommandText = "SELECT 1";
            command.CommandType = CommandType.Text;

            object? result = await command.ExecuteScalarAsync();

            switch (result)
            {
                case long and 1:
                case 1:
                    return Result.Ok();
            }
        }
        catch (Exception e)
        {
            return Result.Fail(e.Message);
        }

        return Result.Fail("Connection test failed.");
    }

    public static async Task<Result<DatabaseExecutionResult>> QueryOnUserDatabaseAsync(
        DatabaseManagementSystem databaseManagementSystem,
        string connectionString,
        string query)
    {
        try
        {
            await using DbConnection connection = GetDatabaseConnection(databaseManagementSystem, connectionString);

            await connection.OpenAsync();

            await using DbCommand command = connection.CreateCommand();

            command.CommandText = query;
            command.CommandType = CommandType.Text;

            var stopwatch = Stopwatch.StartNew();

            DbDataReader dbDataReader = await command.ExecuteReaderAsync();

            stopwatch.Stop();

            DataTable dataTable = new();
            dataTable.Load(dbDataReader);

            DatabaseExecutionResult databaseExecutionResult = ConvertDataTableToResult(dataTable, stopwatch.Elapsed);

            return Result<DatabaseExecutionResult>.Ok(databaseExecutionResult);
        }
        catch (Exception e)
        {
            return Result<DatabaseExecutionResult>.Fail(e.Message);
        }
    }

    private static DatabaseExecutionResult ConvertDataTableToResult(
        DataTable dataTable,
        TimeSpan elapsedTime)
    {
        var columnNames = dataTable.Columns
            .Cast<DataColumn>()
            .Select(c => c.ColumnName)
            .ToList();

        List<Dictionary<string, object>> values = [];
        foreach (DataRow row in dataTable.Rows)
        {
            Dictionary<string, object> rowDictionary = new();
            foreach (DataColumn column in dataTable.Columns) rowDictionary[column.ColumnName] = row[column];

            values.Add(rowDictionary);
        }

        return new DatabaseExecutionResult(dataTable.Rows.Count, elapsedTime.Milliseconds, columnNames, values);
    }

    public static Result<string> ValidateSqlQuerySyntax(DatabaseManagementSystem databaseManagementSystem, string query)
    {
        try
        {
            ICharStream inputStream = new AntlrInputStream(query);
            Lexer lexer = AntlrFactory.CreateLexer(databaseManagementSystem, inputStream);
            CommonTokenStream tokenStream = new CommonTokenStream(lexer);
            Parser parser = AntlrFactory.CreateParser(databaseManagementSystem, tokenStream);

            var syntaxErrorListener = new SyntaxErrorListener();
            parser.RemoveErrorListeners();
            parser.AddErrorListener(syntaxErrorListener);

            parser.GetType().GetMethod(parser.RuleNames[0])?.Invoke(parser, null);

            return parser.NumberOfSyntaxErrors == 0
                ? Result<string>.Ok("Syntax is valid!")
                : Result<string>.Fail(string.Join("; ", syntaxErrorListener
                    .Errors
                    .Select(e => $"Error: {e.Message} at line {e.Line}, column {e.Column}")));
        }
        catch (Exception e)
        {
            return Result<string>.Fail(e.Message);
        }
    }
}
