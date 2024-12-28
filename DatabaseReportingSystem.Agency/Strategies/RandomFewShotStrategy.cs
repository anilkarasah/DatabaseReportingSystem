using DatabaseReportingSystem.Shared;
using DatabaseReportingSystem.Vector.Features;
using Microsoft.Extensions.DependencyInjection;
using OpenAI.Chat;

namespace DatabaseReportingSystem.Agency.Strategies;

public sealed class RandomFewShotStrategy(RandomFewShotStrategy.Options options) : IStrategy
{
    public sealed class Options
    {
        public required IServiceProvider ServiceProvider { get; init; }

        public int NumberOfExamples { get; init; } = Constants.Strategy.DefaultNumberOfExamples;

        public bool UseSystemPrompt { get; init; } = true;
    }

    private readonly Options _options = options;

    public async Task<List<ChatMessage>> GetMessagesAsync()
    {
        var randomQuestions = await QueryRandomQuestionsAsync();

        List<ChatMessage> result = [];
        
        if (_options.UseSystemPrompt)
        {
            result.Add(new SystemChatMessage(Constants.Strategy.BaseSystemPromptMessage));
        }

        foreach (GetRandomQuestions.Response randomQuestion in randomQuestions)
        {
            UserChatMessage userMessage = Utilities.CreateUserChatMessage(
                randomQuestion.Question,
                randomQuestion.Schema,
                "dbms");

            result.Add(userMessage);

            result.Add(new AssistantChatMessage(randomQuestion.Query));
        }

        return result;
    }

    private async Task<List<GetRandomQuestions.Response>> QueryRandomQuestionsAsync()
    {
        var feature = _options.ServiceProvider.GetRequiredService<GetRandomQuestions.Feature>();

        var request = new GetRandomQuestions.Request(_options.NumberOfExamples);

        return await feature.GetRandomQuestionsAsync(request);
    }
}
