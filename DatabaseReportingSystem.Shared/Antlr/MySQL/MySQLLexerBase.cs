/*
 * Copyright © 2024, Oracle and/or its affiliates
 */

/* eslint-disable no-underscore-dangle */
/* cspell: ignore antlr, longlong, ULONGLONG, MAXDB */

using Antlr4.Runtime;

namespace DatabaseReportingSystem.Shared.Antlr.MySQL;

/** The base lexer class provides a number of functions needed in actions in the lexer (grammar). */
public class MySqlLexerBase : Lexer
{
    public int ServerVersion;
    public HashSet<SqlMode> SqlModes;

    /** Enable Multi Language Extension support. */
    public bool SupportMle = true;

    private bool _justEmittedDot;

    public HashSet<string> CharSets = []; // Used to check repertoires.
    protected bool InVersionComment;

    private readonly Queue<IToken?> _pendingTokens = new Queue<IToken?>();

    private const string LongString = "2147483647";
    private const int LongLength = 10;
    private const string SignedLongString = "-2147483648";
    private const string LongLongString = "9223372036854775807";
    private const int LongLongLength = 19;
    private const string SignedLongLongString = "-9223372036854775808";
    private const int SignedLongLongLength = 19;
    private const string UnsignedLongLongString = "18446744073709551615";
    private const int UnsignedLongLongLength = 20;

    public override string[] RuleNames => throw new NotImplementedException();

    public override IVocabulary Vocabulary => throw new NotImplementedException();

    public override string GrammarFileName => throw new NotImplementedException();


    protected MySqlLexerBase(ICharStream input, TextWriter output, TextWriter errorOutput)
        : base(input, output, errorOutput)
    {
        ServerVersion = 80200;
        SqlModes = MySQL.SqlModes.SqlModeFromString("ANSI_QUOTES");
    }

    public MySqlLexerBase(ICharStream input)
        : base(input)
    {
        ServerVersion = 80200;
        SqlModes = MySQL.SqlModes.SqlModeFromString("ANSI_QUOTES");
    }

    /**
     * Determines if the given SQL mode is currently active in the lexer.
     *
     * @param mode The mode to check.
     *
     * @returns True if the mode is one of the currently active modes.
     */
    public bool IsSqlModeActive(SqlMode mode)
    {
        return SqlModes.Contains(mode);
    }

    /**
     * Resets the lexer by setting initial values to transient member, resetting the input stream position etc.
     */
    public override void Reset()
    {
        InVersionComment = false;
        base.Reset();
    }

    /**
     * Implements the multi token feature required in our lexer.
     * A lexer rule can emit more than a single token, if needed.
     *
     * @returns The next token in the token stream.
     */
    public override IToken? NextToken()
    {
        // First respond with pending tokens to the next token request, if there are any.
        bool notEmpty = _pendingTokens.TryDequeue(out IToken? pending);
        if (notEmpty)
        {
            return pending;
        }

        // Let the main lexer class run the next token recognition.
        // This might create additional tokens again.
        IToken? next = base.NextToken();

        notEmpty = _pendingTokens.TryDequeue(out pending);
        if (!notEmpty) return next;
        _pendingTokens.Enqueue(next);
        return pending;
    }

    /**
     * Checks if the version number in the token text is less than or equal to the current server version.
     *
     * @param text The text from a matched token.
     * @returns True if so the number matches, otherwise false.
     */
    protected bool CheckMySqlVersion(string text)
    {
        if (text.Length < 8)
        {
            // Minimum is: /*!12345
            return false;
        }

        // Skip version comment introducer.
        int version = Int32.Parse(text.Substring(3));
        if (version <= ServerVersion)
        {
            InVersionComment = true;

            return true;
        }

        return false;
    }

    /**
     * Called when a keyword was consumed that represents an internal MySQL function and checks if that keyword is
     * followed by an open parenthesis. If not then it is not considered a keyword but treated like a normal identifier.
     *
     * @param proposed The token type to use if the check succeeds.
     *
     * @returns If a function call is found then return the proposed token type, otherwise just IDENTIFIER.
     */
    protected int DetermineFunction(int proposed)
    {
        // Skip any whitespace character if the sql mode says they should be ignored,
        // before actually trying to match the open parenthesis.
        char input = (char)InputStream.LA(1);
        if (IsSqlModeActive(SqlMode.IgnoreSpace))
        {
            while (input == ' ' || input == '\t' || input == '\r' || input == '\n')
            {
                Interpreter.Consume((ICharStream)InputStream);
                Channel = Hidden;
                Type = MySQLLexer.WHITESPACE;
                input = (char)InputStream.LA(1);
            }
        }

        return input == '(' ? proposed : MySQLLexer.IDENTIFIER;
    }

    /**
     * Checks the given text and determines the smallest number type from it. Code has been taken from sql_lex.cc.
     *
     * @param text The text to parse (which must be a number).
     *
     * @returns The token type for that text.
     */
    protected int DetermineNumericType(string text)
    {
        // The original code checks for leading +/- but actually that can never happen, neither in the
        // server parser (as a digit is used to trigger processing in the lexer) nor in our parser
        // as our rules are defined without signs. But we do it anyway for maximum compatibility.
        int length = text.Length - 1;
        if (length < LongLength)
        {
            // quick normal case
            return MySQLLexer.INT_NUMBER;
        }

        bool negative = false;
        int index = 0;
        if (text[index] == '+')
        {
            // Remove sign and pre-zeros
            ++index;
            --length;
        }
        else if (text[index] == '-')
        {
            ++index;
            --length;
            negative = true;
        }

        while (text[index] == '0' && length > 0)
        {
            ++index;
            --length;
        }

        if (length < LongLength)
        {
            return MySQLLexer.INT_NUMBER;
        }

        int smaller;
        int bigger;
        string cmp;
        if (negative)
        {
            if (length == LongLength)
            {
                cmp = SignedLongString.Substring(1);
                smaller = MySQLLexer.INT_NUMBER; // If <= signed_long_str
                bigger = MySQLLexer.LONG_NUMBER; // If >= signed_long_str
            }
            else if (length < SignedLongLongLength)
            {
                return MySQLLexer.LONG_NUMBER;
            }
            else if (length > SignedLongLongLength)
            {
                return MySQLLexer.DECIMAL_NUMBER;
            }
            else
            {
                cmp = SignedLongLongString.Substring(1);
                smaller = MySQLLexer.LONG_NUMBER; // If <= signed_longlong_str
                bigger = MySQLLexer.DECIMAL_NUMBER;
            }
        }
        else
        {
            if (length == LongLength)
            {
                cmp = LongString;
                smaller = MySQLLexer.INT_NUMBER;
                bigger = MySQLLexer.LONG_NUMBER;
            }
            else if (length < LongLongLength)
            {
                return MySQLLexer.LONG_NUMBER;
            }
            else if (length > LongLongLength)
            {
                if (length > UnsignedLongLongLength)
                {
                    return MySQLLexer.DECIMAL_NUMBER;
                }

                cmp = UnsignedLongLongString;
                smaller = MySQLLexer.ULONGLONG_NUMBER;
                bigger = MySQLLexer.DECIMAL_NUMBER;
            }
            else
            {
                cmp = LongLongString;
                smaller = MySQLLexer.LONG_NUMBER;
                bigger = MySQLLexer.ULONGLONG_NUMBER;
            }
        }

        int otherIndex = 0;
        while (index < text.Length && cmp[otherIndex++] == text[index++])
        {
            //
        }

        return text[index - 1] <= cmp[otherIndex - 1] ? smaller : bigger;
    }

    /**
     * Checks if the given text corresponds to a charset defined in the server (text is preceded by an underscore).
     *
     * @param text The text to check.
     *
     * @returns UNDERSCORE_CHARSET if so, otherwise IDENTIFIER.
     */
    protected int CheckCharset(string text)
    {
        return CharSets.Contains(text) ? MySQLLexer.UNDERSCORE_CHARSET : MySQLLexer.IDENTIFIER;
    }

    /**
     * Creates a DOT token in the token stream.
     */
    protected void EmitDot()
    {
        int len = Text.Length;
        var t = TokenFactory.Create(new Tuple<ITokenSource, ICharStream>(this, (ICharStream)InputStream),
            MySQLLexer.DOT_SYMBOL,
            ".", Channel, TokenStartCharIndex, TokenStartCharIndex, Line,
            Column) as CommonToken;
        _pendingTokens.Enqueue(t);
        t.Column -= len;
        ++Column;
        _justEmittedDot = true;
    }

    public override IToken Emit()
    {
        IToken? t = base.Emit();
        if (!_justEmittedDot) return t;

        var p = t as CommonToken;
        p.Text = p.Text.Substring(1);
        p.Column += 1;
        p.StartIndex += 1;
        Column -= 1;
        _justEmittedDot = false;

        return t;
    }

    // Version-related methods
    public bool IsServerVersionLt80024() => ServerVersion < 80024;
    public bool IsServerVersionGe80024() => ServerVersion >= 80024;
    public bool IsServerVersionGe80011() => ServerVersion >= 80011;
    public bool IsServerVersionGe80013() => ServerVersion >= 80013;
    public bool IsServerVersionLt80014() => ServerVersion < 80014;
    public bool IsServerVersionGe80014() => ServerVersion >= 80014;
    public bool IsServerVersionGe80017() => ServerVersion >= 80017;
    public bool IsServerVersionGe80018() => ServerVersion >= 80018;

    public bool IsMasterCompressionAlgorithm() => ServerVersion >= 80018 && IsServerVersionLt80024();

    public bool IsServerVersionLt80031() => ServerVersion < 80031;

    // Functions for specific token types
    public void DoLogicalOr()
    {
        Type = IsSqlModeActive(SqlMode.PipesAsConcat) ? MySQLLexer.CONCAT_PIPES_SYMBOL : MySQLLexer.LOGICAL_OR_OPERATOR;
    }

    public void DoIntNumber()
    {
        Type = DetermineNumericType(Text);
    }

    public void DoAdddate() => Type = DetermineFunction(MySQLLexer.ADDDATE_SYMBOL);
    public void DoBitAnd() => Type = DetermineFunction(MySQLLexer.BIT_AND_SYMBOL);
    public void DoBitOr() => Type = DetermineFunction(MySQLLexer.BIT_OR_SYMBOL);
    public void DoBitXor() => Type = DetermineFunction(MySQLLexer.BIT_XOR_SYMBOL);
    public void DoCast() => Type = DetermineFunction(MySQLLexer.CAST_SYMBOL);
    public void DoCount() => Type = DetermineFunction(MySQLLexer.COUNT_SYMBOL);
    public void DoCurdate() => Type = DetermineFunction(MySQLLexer.CURDATE_SYMBOL);
    public void DoCurrentDate() => Type = DetermineFunction(MySQLLexer.CURDATE_SYMBOL);
    public void DoCurrentTime() => Type = DetermineFunction(MySQLLexer.CURTIME_SYMBOL);
    public void DoCurtime() => Type = DetermineFunction(MySQLLexer.CURTIME_SYMBOL);
    public void DoDateAdd() => Type = DetermineFunction(MySQLLexer.DATE_ADD_SYMBOL);
    public void DoDateSub() => Type = DetermineFunction(MySQLLexer.DATE_SUB_SYMBOL);
    public void DoExtract() => Type = DetermineFunction(MySQLLexer.EXTRACT_SYMBOL);
    public void DoGroupConcat() => Type = DetermineFunction(MySQLLexer.GROUP_CONCAT_SYMBOL);
    public void DoMax() => Type = DetermineFunction(MySQLLexer.MAX_SYMBOL);
    public void DoMid() => Type = DetermineFunction(MySQLLexer.SUBSTRING_SYMBOL);
    public void DoMin() => Type = DetermineFunction(MySQLLexer.MIN_SYMBOL);

    public void DoNot()
        => Type = IsSqlModeActive(SqlMode.HighNotPrecedence) ? MySQLLexer.NOT2_SYMBOL : MySQLLexer.NOT_SYMBOL;

    public void DoNow() => Type = DetermineFunction(MySQLLexer.NOW_SYMBOL);
    public void DoPosition() => Type = DetermineFunction(MySQLLexer.POSITION_SYMBOL);
    public void DoSessionUser() => Type = DetermineFunction(MySQLLexer.USER_SYMBOL);
    public void DoStddevSamp() => Type = DetermineFunction(MySQLLexer.STDDEV_SAMP_SYMBOL);
    public void DoStddev() => Type = DetermineFunction(MySQLLexer.STD_SYMBOL);
    public void DoStddevPop() => Type = DetermineFunction(MySQLLexer.STD_SYMBOL);
    public void DoStd() => Type = DetermineFunction(MySQLLexer.STD_SYMBOL);
    public void DoSubdate() => Type = DetermineFunction(MySQLLexer.SUBDATE_SYMBOL);
    public void DoSubstr() => Type = DetermineFunction(MySQLLexer.SUBSTRING_SYMBOL);
    public void DoSubstring() => Type = DetermineFunction(MySQLLexer.SUBSTRING_SYMBOL);
    public void DoSum() => Type = DetermineFunction(MySQLLexer.SUM_SYMBOL);
    public void DoSysdate() => Type = DetermineFunction(MySQLLexer.SYSDATE_SYMBOL);
    public void DoSystemUser() => Type = DetermineFunction(MySQLLexer.USER_SYMBOL);
    public void DoTrim() => Type = DetermineFunction(MySQLLexer.TRIM_SYMBOL);
    public void DoVariance() => Type = DetermineFunction(MySQLLexer.VARIANCE_SYMBOL);
    public void DoVarPop() => Type = DetermineFunction(MySQLLexer.VARIANCE_SYMBOL);
    public void DoVarSamp() => Type = DetermineFunction(MySQLLexer.VAR_SAMP_SYMBOL);
    public void DoUnderscoreCharset() => Type = CheckCharset(Text);

    public bool IsVersionComment() => CheckMySqlVersion(Text);

    public bool IsBackTickQuotedId()
    {
        return !IsSqlModeActive(SqlMode.NoBackslashEscapes);
    }

    public bool IsDoubleQuotedText()
    {
        return !IsSqlModeActive(SqlMode.NoBackslashEscapes);
    }

    public bool IsSingleQuotedText()
    {
        return !IsSqlModeActive(SqlMode.NoBackslashEscapes);
    }

    public void StartInVersionComment()
    {
        InVersionComment = true;
    }

    public void EndInVersionComment()
    {
        InVersionComment = false;
    }

    public bool IsInVersionComment()
    {
        return InVersionComment;
    }
}
