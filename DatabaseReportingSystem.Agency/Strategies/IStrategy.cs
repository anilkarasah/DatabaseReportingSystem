namespace DatabaseReportingSystem.Agency.Strategies;

public interface IStrategy
{
    Task<string> GetMessagesAsync();
}
