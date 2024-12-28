using OpenAI.Chat;

namespace DatabaseReportingSystem.Agency.LanguageModels;

public interface ILanguageModel
{
    Task<string> AskAsync(List<ChatMessage> chatMessages);
}
