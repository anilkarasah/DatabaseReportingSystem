using System.Collections.Immutable;
using Antlr4.Runtime;

namespace DatabaseReportingSystem.Shared.Antlr;

public sealed class SyntaxErrorListener : IAntlrErrorListener<IToken>
{
    private readonly List<SyntaxError> _errors = [];

    public ImmutableList<SyntaxError> Errors => _errors.ToImmutableList();

    public void SyntaxError(
        TextWriter output,
        IRecognizer recognizer,
        IToken offendingSymbol,
        int line,
        int charPositionInLine,
        string msg,
        RecognitionException e)
    {
        _errors.Add(new SyntaxError
        {
            Line = line,
            Column = charPositionInLine,
            Message = msg,
            OffendingSymbol = offendingSymbol?.Text
        });
    }
}

public sealed class SyntaxError
{
    public int Line { get; set; }
    public int Column { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? OffendingSymbol { get; set; }
}
