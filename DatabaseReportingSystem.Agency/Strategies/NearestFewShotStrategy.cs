using DatabaseReportingSystem.Shared;
using DatabaseReportingSystem.Vector.Features;
using Microsoft.Extensions.DependencyInjection;
using OpenAI.Chat;

namespace DatabaseReportingSystem.Agency.Strategies;

public sealed class NearestFewShotStrategy(NearestFewShotStrategy.Options options) : IStrategy
{
    public sealed class Options
    {
        public required IServiceProvider ServiceProvider { get; init; }

        public required string Question { get; init; }

        public int NumberOfExamples { get; init; } = Constants.Strategy.DefaultNumberOfExamples;

        public bool UseSystemPrompt { get; init; } = true;
    }

    private readonly Options _options = options;

    public async Task<List<ChatMessage>> GetMessagesAsync()
    {
        var nearestQuestions = await QueryNearestQuestionsAsync();

        List<ChatMessage> result = [];

        if (_options.UseSystemPrompt)
        {
            result.Add(new SystemChatMessage(Constants.Strategy.BaseSystemPromptMessage));
        }

        foreach (GetNearestQuestions.NearestQuestionDto nearestQuestion in nearestQuestions)
        {
            UserChatMessage userMessage = Utilities.CreateUserChatMessage(
                nearestQuestion.Question,
                nearestQuestion.Schema,
                "dbms");

            result.Add(userMessage);

            result.Add(new AssistantChatMessage(nearestQuestion.Query));
        }

        return result;
    }

    private async Task<List<GetNearestQuestions.NearestQuestionDto>> QueryNearestQuestionsAsync()
    {
        var feature = _options.ServiceProvider.GetRequiredService<GetNearestQuestions.Feature>();

        var request = new GetNearestQuestionsRequest(_options.Question, _options.NumberOfExamples);

        return await feature.GetNearestQuestionsAsync(request);
    }
}
