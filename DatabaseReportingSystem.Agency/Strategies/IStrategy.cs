using OpenAI.Chat;

namespace DatabaseReportingSystem.Agency.Strategies;

public interface IStrategy
{
    bool OnlySystemPrompt { get; set; }

    Task<List<ChatMessage>> GetMessagesAsync();
}
