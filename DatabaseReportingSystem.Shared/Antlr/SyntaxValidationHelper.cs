using Antlr4.Runtime;

namespace DatabaseReportingSystem.Shared.Antlr;

public static class SyntaxValidationHelper
{
    public static Result ValidateMySqlQuery(string query)
    {
        var inputStream = new AntlrInputStream(query);
        var lexer = new MySQLLexer(inputStream);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new MySQLParser(tokenStream);

        // try
        // {
        //     var tree = parser.
        // }
        
        return Result.Ok();
    }
}
