using System.ClientModel;
using AutoGen.OpenAI;
using DatabaseReportingSystem.Shared;
using DatabaseReportingSystem.Shared.Settings;
using OpenAI;
using OpenAI.Chat;

namespace DatabaseReportingSystem.Agency.LanguageModels;

public sealed class MistralModel(ApiKeys apiKeys) : ILanguageModel
{
    private readonly OpenAIClientOptions _openAiClientOptions = new()
    {
        Endpoint = new Uri(apiKeys.ApiUrl)
    };

    private const string ModelName = "mistral";
    private readonly ApiKeyCredential _apiKeyCredential = new("ha-bu-yemdur-xd");

    public async Task<string> AskAsync(List<ChatMessage> chatMessages)
    {
        ChatClient client = CreateClient();

        ChatCompletion completion = await client.CompleteChatAsync(chatMessages, new ChatCompletionOptions
        {
            Temperature = 0
        });

        if (completion.Content.Count == 0) throw new InvalidOperationException("No response from Mistral.");

        return Utilities.TrimSqlString(completion.Content[0].Text.Replace("\\_", "_"));
    }

    public OpenAIChatAgent GetChatAgent(string name, string systemMessage = "")
    {
        return new OpenAIChatAgent(
            name: name,
            systemMessage: systemMessage,
            chatClient: CreateClient(),
            temperature: 1,
            maxTokens: null);
    }

    private ChatClient CreateClient()
    {
        return new OpenAIClient(_apiKeyCredential, _openAiClientOptions).GetChatClient(ModelName);
    }
}
