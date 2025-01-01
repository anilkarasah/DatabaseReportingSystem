using AutoGen.OpenAI;
using OpenAI.Chat;

namespace DatabaseReportingSystem.Agency.LanguageModels;

public sealed class CodeLlamaModel : ILanguageModel
{
    public Task<string> AskAsync(List<ChatMessage> chatMessages)
    {
        throw new NotImplementedException();
    }

    public OpenAIChatAgent GetChatAgent(string name, string systemMessage = "")
    {
        throw new NotImplementedException();
    }
}
