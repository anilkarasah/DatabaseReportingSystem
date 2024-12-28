using System.ClientModel;
using OpenAI.Chat;
using Utilities = DatabaseReportingSystem.Shared.Utilities;

namespace DatabaseReportingSystem.Agency.LanguageModels;

public sealed class GptModel(string modelName, string apiKey) : ILanguageModel
{
    private readonly string _modelName = modelName;
    private readonly ChatClient _client = new(modelName, new ApiKeyCredential(apiKey));

    private static readonly ChatCompletionOptions ChatCompletionOptions = new()
    {
        Temperature = 0,
    };

    public async Task<string> AskAsync(List<ChatMessage> chatMessages)
    {
        ChatCompletion completion = await _client.CompleteChatAsync(chatMessages, ChatCompletionOptions);

        if (completion.Content.Count == 0)
        {
            throw new InvalidOperationException("No response from GPT.");
        }

        return Utilities.TrimSqlString(completion.Content[0].Text);
    }
}
