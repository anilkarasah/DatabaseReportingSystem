using OpenAI.Chat;

namespace DatabaseReportingSystem.Agency.Strategies;

public interface IStrategy
{
    Task<List<ChatMessage>> GetMessagesAsync();
}
