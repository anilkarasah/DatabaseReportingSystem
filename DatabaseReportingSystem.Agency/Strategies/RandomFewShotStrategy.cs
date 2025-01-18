using System.Text;
using DatabaseReportingSystem.Shared;
using DatabaseReportingSystem.Shared.Models;
using DatabaseReportingSystem.Vector.Features;
using Microsoft.Extensions.DependencyInjection;
using OpenAI.Chat;
using ChatMessage = OpenAI.Chat.ChatMessage;

namespace DatabaseReportingSystem.Agency.Strategies;

public sealed class RandomFewShotStrategy(RandomFewShotStrategy.Options options) : IStrategy
{
    private readonly Options _options = options;

    public bool OnlySystemPrompt { get; set; } = false;

    public async Task<List<ChatMessage>> GetMessagesAsync()
    {
        var randomQuestions = await QueryRandomQuestionsAsync();

        var messages = new List<ChatMessage>();

        if (OnlySystemPrompt)
        {
            var stringBuilder = new StringBuilder();

            if (_options.UseSystemPrompt) stringBuilder.AppendLine(Constants.Strategy.BaseSystemPromptMessage);

            int exampleIndex = 1;
            foreach (GetRandomQuestions.GetRandomQuestionsResponse nearestQuestion in randomQuestions)
            {
                string userMessage = Utilities
                    .CreateUserMessage(new UserPromptDto(
                        nearestQuestion.Question,
                        nearestQuestion.Schema,
                        DatabaseManagementSystem.Sqlite))
                    .ReplaceLineEndings(" ");

                stringBuilder.AppendLine($"\n\nExample {exampleIndex}:");
                stringBuilder.AppendLine(userMessage);
                stringBuilder.AppendLine(nearestQuestion.Query);

                exampleIndex++;
            }

            messages.Add(new SystemChatMessage(stringBuilder.ToString()));
        }
        else
        {
            if (_options.UseSystemPrompt) messages.Add(new SystemChatMessage(Constants.Strategy.BaseSystemPromptMessage));

            foreach (GetRandomQuestions.GetRandomQuestionsResponse randomQuestion in randomQuestions)
            {
                string userMessage = Utilities
                    .CreateUserMessage(new UserPromptDto(
                        randomQuestion.Question,
                        randomQuestion.Schema,
                        DatabaseManagementSystem.Sqlite));

                messages.Add(new UserChatMessage(userMessage));
                messages.Add(new AssistantChatMessage(randomQuestion.Query));
            }
        }

        return messages;
    }

    private async Task<List<GetRandomQuestions.GetRandomQuestionsResponse>> QueryRandomQuestionsAsync()
    {
        var feature = _options.ServiceProvider.GetRequiredService<GetRandomQuestions.Feature>();

        var request = new GetRandomQuestions.GetRandomQuestionsRequest(_options.NumberOfExamples);

        return await feature.GetRandomQuestionsAsync(request);
    }

    public sealed class Options
    {
        public required IServiceProvider ServiceProvider { get; init; }

        public int NumberOfExamples { get; init; } = Constants.Strategy.DefaultNumberOfExamples;

        public bool UseSystemPrompt { get; init; } = true;
    }
}
