using System.Text;
using DatabaseReportingSystem.Shared;
using DatabaseReportingSystem.Shared.Models;
using DatabaseReportingSystem.Vector.Features;
using Microsoft.Extensions.DependencyInjection;

namespace DatabaseReportingSystem.Agency.Strategies;

public sealed class NearestFewShotStrategy(NearestFewShotStrategy.Options options) : IStrategy
{
    private readonly Options _options = options;

    public async Task<string> GetMessagesAsync()
    {
        var stringBuilder = new StringBuilder();

        var nearestQuestions = await QueryNearestQuestionsAsync();

        if (_options.UseSystemPrompt) stringBuilder.AppendLine(Constants.Strategy.BaseSystemPromptMessage);

        int exampleCount = 1;
        foreach (GetNearestQuestions.NearestQuestionDto nearestQuestion in nearestQuestions)
        {
            string userMessage = Utilities
                .CreateUserMessage(new UserPromptDto(
                    nearestQuestion.Question,
                    nearestQuestion.Schema,
                    DatabaseManagementSystem.Sqlite))
                .ReplaceLineEndings(" ");

            stringBuilder.AppendLine($"Example {exampleCount}: {userMessage}");
            stringBuilder.AppendLine($"Result {exampleCount}: {nearestQuestion.Query}");

            exampleCount++;
        }

        return stringBuilder.ToString();
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
