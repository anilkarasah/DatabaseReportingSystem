namespace DatabaseReportingSystem.Shared.Antlr.MySQL;

/** SQL modes that control parsing behavior. */
public enum SqlMode
{
    NoMode,
    AnsiQuotes,
    HighNotPrecedence,
    PipesAsConcat,
    IgnoreSpace,
    NoBackslashEscapes,
}
