using System.ClientModel;
using AutoGen.OpenAI;
using DatabaseReportingSystem.Shared;
using OpenAI;
using OpenAI.Chat;

namespace DatabaseReportingSystem.Agency.LanguageModels;

public sealed class GrokModel(string modelName, string apiKey) : ILanguageModel
{
    private static readonly Uri GrokUri = new("https://api.x.ai/v1");

    private static readonly OpenAIClientOptions OpenAiClientOptions = new()
    {
        Endpoint = GrokUri
    };

    private readonly ApiKeyCredential _credential = new(apiKey);
    private readonly string _modelName = modelName;

    public async Task<string> AskAsync(List<ChatMessage> chatMessages)
    {
        ChatClient client = CreateClient();

        ChatCompletion completion = await client.CompleteChatAsync(chatMessages, new ChatCompletionOptions
        {
            Temperature = 0
        });

        if (completion.Content.Count == 0) throw new InvalidOperationException("No response from Grok.");

        return Utilities.TrimSqlString(completion.Content[0].Text);
    }

    public OpenAIChatAgent GetChatAgent(string name, string systemMessage = "")
    {
        return new OpenAIChatAgent(name: name, systemMessage: systemMessage, chatClient: CreateClient());
    }

    private ChatClient CreateClient()
    {
        return new OpenAIClient(_credential, OpenAiClientOptions).GetChatClient(_modelName);
    }
}
