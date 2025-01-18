/*
 * Copyright © 2024, Oracle and/or its affiliates
 */

using Antlr4.Runtime;

namespace DatabaseReportingSystem.Shared.Antlr.MySQL;

public abstract class MySqlParserBase : Parser
{
    // To parameterize the parsing process.
    public int ServerVersion = 0;
    public HashSet<SqlMode> SqlModes;

    /** Enable Multi Language Extension support. */
    public bool SupportMle = true;

    protected MySqlParserBase(ITokenStream input, TextWriter output, TextWriter errorOutput) : base(input, output,
        errorOutput)
    {
        ServerVersion = 80200;
        SqlModes = MySQL.SqlModes.SqlModeFromString("ANSI_QUOTES");
    }

    public bool IsSqlModeActive(SqlMode mode)
    {
        return SqlModes.Contains(mode);
    }

    public bool IsPureIdentifier()
    {
        return IsSqlModeActive(SqlMode.AnsiQuotes);
    }

    public bool IsTextStringLiteral()
    {
        return !IsSqlModeActive(SqlMode.AnsiQuotes);
    }

    public bool IsStoredRoutineBody()
    {
        return ServerVersion >= 80032 && SupportMle;
    }

    public bool IsSelectStatementWithInto()
    {
        return ServerVersion >= 80024 && ServerVersion < 80031;
    }

    public bool IsServerVersionGe80004()
    {
        return ServerVersion >= 80004;
    }

    public bool IsServerVersionGe80011()
    {
        return ServerVersion >= 80011;
    }

    public bool IsServerVersionGe80013()
    {
        return ServerVersion >= 80013;
    }

    public bool IsServerVersionGe80014()
    {
        return ServerVersion >= 80014;
    }

    public bool IsServerVersionGe80016()
    {
        return ServerVersion >= 80016;
    }

    public bool IsServerVersionGe80017()
    {
        return ServerVersion >= 80017;
    }

    public bool IsServerVersionGe80018()
    {
        return ServerVersion >= 80018;
    }

    public bool IsServerVersionGe80019()
    {
        return ServerVersion >= 80019;
    }

    public bool IsServerVersionGe80024()
    {
        return ServerVersion >= 80024;
    }

    public bool IsServerVersionGe80025()
    {
        return ServerVersion >= 80025;
    }

    public bool IsServerVersionGe80027()
    {
        return ServerVersion >= 80027;
    }

    public bool IsServerVersionGe80031()
    {
        return ServerVersion >= 80031;
    }

    public bool IsServerVersionGe80032()
    {
        return ServerVersion >= 80032;
    }

    public bool IsServerVersionGe80100()
    {
        return ServerVersion >= 80100;
    }

    public bool IsServerVersionGe80200()
    {
        return ServerVersion >= 80200;
    }

    public bool IsServerVersionLt80011()
    {
        return ServerVersion < 80011;
    }

    public bool IsServerVersionLt80012()
    {
        return ServerVersion < 80012;
    }

    public bool IsServerVersionLt80014()
    {
        return ServerVersion < 80014;
    }

    public bool IsServerVersionLt80016()
    {
        return ServerVersion < 80016;
    }

    public bool IsServerVersionLt80017()
    {
        return ServerVersion < 80017;
    }

    public bool IsServerVersionLt80024()
    {
        return ServerVersion < 80024;
    }

    public bool IsServerVersionLt80025()
    {
        return ServerVersion < 80025;
    }

    public bool IsServerVersionLt80031()
    {
        return ServerVersion < 80031;
    }
}
