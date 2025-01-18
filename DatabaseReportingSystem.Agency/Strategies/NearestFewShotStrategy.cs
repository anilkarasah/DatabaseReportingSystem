using System.Text;
using DatabaseReportingSystem.Shared;
using DatabaseReportingSystem.Shared.Models;
using DatabaseReportingSystem.Vector.Features;
using Microsoft.Extensions.DependencyInjection;
using OpenAI.Chat;
using ChatMessage = OpenAI.Chat.ChatMessage;

namespace DatabaseReportingSystem.Agency.Strategies;

public sealed class NearestFewShotStrategy(NearestFewShotStrategy.Options options) : IStrategy
{
    private readonly Options _options = options;

    public bool OnlySystemPrompt { get; set; } = false;

    public async Task<List<ChatMessage>> GetMessagesAsync()
    {
        var nearestQuestions = await QueryNearestQuestionsAsync();

        var messages = new List<ChatMessage>();

        if (OnlySystemPrompt)
        {
            var stringBuilder = new StringBuilder();

            if (_options.UseSystemPrompt) stringBuilder.AppendLine(Constants.Strategy.BaseSystemPromptMessage);

            int exampleIndex = 1;
            foreach (GetNearestQuestions.NearestQuestionDto nearestQuestion in nearestQuestions)
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
            if (_options.UseSystemPrompt)
                messages.Add(new SystemChatMessage(Constants.Strategy.BaseSystemPromptMessage));

            foreach (GetNearestQuestions.NearestQuestionDto nearestQuestion in nearestQuestions)
            {
                string userMessage = Utilities
                    .CreateUserMessage(new UserPromptDto(
                        nearestQuestion.Question,
                        nearestQuestion.Schema,
                        DatabaseManagementSystem.Sqlite))
                    .ReplaceLineEndings(" ");

                messages.Add(new UserChatMessage(userMessage));
                messages.Add(new AssistantChatMessage(nearestQuestion.Query));
            }
        }

        return messages;
    }

    private async Task<List<GetNearestQuestions.NearestQuestionDto>> QueryNearestQuestionsAsync()
    {
        var feature = _options.ServiceProvider.GetRequiredService<GetNearestQuestions.Feature>();

        var request = new GetNearestQuestionsRequest(_options.Question, _options.NumberOfExamples);

        return await feature.GetNearestQuestionsAsync(request);
    }

    public sealed class Options
    {
        public required IServiceProvider ServiceProvider { get; init; }

        public required string Question { get; init; }

        public int NumberOfExamples { get; init; } = Constants.Strategy.DefaultNumberOfExamples;

        public bool UseSystemPrompt { get; init; } = true;
    }
}
