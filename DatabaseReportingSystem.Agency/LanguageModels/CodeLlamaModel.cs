using OpenAI.Chat;

namespace DatabaseReportingSystem.Agency.LanguageModels;

public sealed class CodeLlamaModel : ILanguageModel
{
    public Task<string> AskAsync(List<ChatMessage> chatMessages)
    {
        throw new NotImplementedException();
    }
}
