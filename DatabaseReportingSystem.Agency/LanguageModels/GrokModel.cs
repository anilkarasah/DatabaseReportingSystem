using System.ClientModel;
using DatabaseReportingSystem.Shared;
using OpenAI;
using OpenAI.Chat;

namespace DatabaseReportingSystem.Agency.LanguageModels;

public sealed class GrokModel(string modelName, string apiKey) : ILanguageModel
{
    private readonly string _modelName = modelName;
    private readonly ApiKeyCredential _credential = new(apiKey);

    private static readonly Uri GrokUri = new("https://api.x.ai/v1");

    private static readonly OpenAIClientOptions OpenAiClientOptions = new()
    {
        Endpoint = GrokUri
    };

    private static readonly ChatCompletionOptions ChatCompletionOptions = new()
    {
        Temperature = 0,
    };

    private ChatClient CreateClient()
    {
        return new OpenAIClient(_credential, OpenAiClientOptions).GetChatClient(_modelName);
    }

    public async Task<string> AskAsync(List<ChatMessage> chatMessages)
    {
        ChatClient client = CreateClient();

        ChatCompletion completion = await client.CompleteChatAsync(chatMessages, ChatCompletionOptions);

        if (completion.Content.Count == 0)
        {
            throw new InvalidOperationException("No response from Grok.");
        }

        return Utilities.TrimSqlString(completion.Content[0].Text);
    }
}
