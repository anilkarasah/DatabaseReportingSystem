namespace DatabaseReportingSystem.Shared.Antlr.MySQL;

public class SqlModes
{
    /**
     * Converts a mode string into individual mode flags.
     *
     * @param modes The input string to parse.
     */
    public static HashSet<SqlMode> SqlModeFromString(string modes)
    {
        var result = new HashSet<SqlMode>();
        string[] parts = modes.ToUpper().Split(",");
        foreach (string mode in parts)
        {
            switch (mode)
            {
                case "ANSI":
                case "DB2":
                case "MAXDB":
                case "MSSQL":
                case "ORACLE":
                case "POSTGRESQL":
                    result.Add(SqlMode.AnsiQuotes);
                    result.Add(SqlMode.PipesAsConcat);
                    result.Add(SqlMode.IgnoreSpace);
                    break;
                case "ANSI_QUOTES":
                    result.Add(SqlMode.AnsiQuotes);
                    break;
                case "PIPES_AS_CONCAT":
                    result.Add(SqlMode.PipesAsConcat);
                    break;
                case "NO_BACKSLASH_ESCAPES":
                    result.Add(SqlMode.NoBackslashEscapes);
                    break;
                case "IGNORE_SPACE":
                    result.Add(SqlMode.IgnoreSpace);
                    break;
                case "HIGH_NOT_PRECEDENCE":
                case "MYSQL323":
                case "MYSQL40":
                    result.Add(SqlMode.HighNotPrecedence);
                    break;
            }
        }

        return result;
    }
}
