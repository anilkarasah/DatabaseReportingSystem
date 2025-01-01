namespace DatabaseReportingSystem.Shared.Models;

[Flags]
public enum StrategyType
{
    ZeroShot = 1 << 0,
    RandomFewShot = 1 << 1,
    NearestFewShot = 1 << 2,
    DailSql = 1 << 3
}
