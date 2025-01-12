using System.Text;
using DatabaseReportingSystem.Shared;
using DatabaseReportingSystem.Shared.Models;
using DatabaseReportingSystem.Vector.Features;
using Microsoft.Extensions.DependencyInjection;

namespace DatabaseReportingSystem.Agency.Strategies;

public sealed class RandomFewShotStrategy(RandomFewShotStrategy.Options options) : IStrategy
{
    private readonly Options _options = options;

    public async Task<string> GetMessagesAsync()
    {
        var stringBuilder = new StringBuilder();

        var randomQuestions = await QueryRandomQuestionsAsync();

        if (_options.UseSystemPrompt) stringBuilder.AppendLine(Constants.Strategy.BaseSystemPromptMessage);

        int exampleCount = 1;
        foreach (GetRandomQuestions.GetRandomQuestionsResponse randomQuestion in randomQuestions)
        {
            string userMessage = Utilities
                .CreateUserMessage(new UserPromptDto(
                    randomQuestion.Question,
                    randomQuestion.Schema,
                    DatabaseManagementSystem.Sqlite));

            stringBuilder.AppendLine($"Example {exampleCount}:");
            stringBuilder.AppendLine(userMessage);
            stringBuilder.AppendLine(randomQuestion.Query);

            exampleCount++;
        }

        return stringBuilder.ToString();
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
