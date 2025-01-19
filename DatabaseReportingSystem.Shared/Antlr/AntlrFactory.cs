using Antlr4.Runtime;
using DatabaseReportingSystem.Shared.Models;

namespace DatabaseReportingSystem.Shared.Antlr;

public static class AntlrFactory
{
    public static Lexer CreateLexer(DatabaseManagementSystem databaseManagementSystem, ICharStream input)
    {
        return databaseManagementSystem switch
        {
            DatabaseManagementSystem.MySql => new MySQLLexer(input),
            DatabaseManagementSystem.PostgreSql => new PostgreSQLLexer(input),
            DatabaseManagementSystem.Sqlite => new SQLiteLexer(input),
            DatabaseManagementSystem.SqlServer => new TSqlLexer(input),
            _ => throw new ArgumentException($"Unsupported DBMS: {databaseManagementSystem}")
        };
    }

    public static Parser CreateParser(DatabaseManagementSystem databaseManagementSystem, CommonTokenStream tokens)
    {
        return databaseManagementSystem switch
        {
            DatabaseManagementSystem.MySql => new MySQLParser(tokens),
            DatabaseManagementSystem.PostgreSql => new PostgreSQLParser(tokens),
            DatabaseManagementSystem.Sqlite => new SQLiteParser(tokens),
            DatabaseManagementSystem.SqlServer => new TSqlParser(tokens),
            _ => throw new ArgumentException($"Unsupported DBMS: {databaseManagementSystem}")
        };
    }
}
