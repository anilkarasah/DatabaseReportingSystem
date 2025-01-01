using System.ClientModel;
using AutoGen.OpenAI;
using OpenAI.Chat;
using Utilities = DatabaseReportingSystem.Shared.Utilities;

namespace DatabaseReportingSystem.Agency.LanguageModels;

public sealed class GptModel(string modelName, string apiKey) : ILanguageModel
{
    private readonly ApiKeyCredential _apiKeyCredential = new(apiKey);
    private readonly string _modelName = modelName;

    public async Task<string> AskAsync(List<ChatMessage> chatMessages)
    {
        ChatClient client = CreateClient();

        ChatCompletion completion = await client.CompleteChatAsync(chatMessages, new ChatCompletionOptions
        {
            Temperature = 0
        });

        if (completion.Content.Count == 0) throw new InvalidOperationException("No response from GPT.");

        return Utilities.TrimSqlString(completion.Content[0].Text);
    }

    public OpenAIChatAgent GetChatAgent(string name, string systemMessage = "")
    {
        return new OpenAIChatAgent(name: name, systemMessage: systemMessage, chatClient: CreateClient(), temperature: 1,
            maxTokens: null);
    }

    private ChatClient CreateClient()
    {
        return new ChatClient(_modelName, _apiKeyCredential);
    }
}
