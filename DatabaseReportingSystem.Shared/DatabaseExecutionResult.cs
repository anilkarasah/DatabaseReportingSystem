namespace DatabaseReportingSystem.Shared;

public sealed record DatabaseExecutionResult(
    int RowCount,
    long ElapsedMilliseconds,
    List<string> ColumnNames,
    List<Dictionary<string, object>> Values);
