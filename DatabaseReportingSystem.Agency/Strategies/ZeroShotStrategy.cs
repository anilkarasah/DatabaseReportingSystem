using DatabaseReportingSystem.Shared;
using OpenAI.Chat;

namespace DatabaseReportingSystem.Agency.Strategies;

public sealed class ZeroShotStrategy(ZeroShotStrategy.Options options) : IStrategy
{
    public sealed class Options
    {
        public bool UseSystemPrompt { get; init; } = true;
    }

    private readonly Options _options = options;

    public Task<List<ChatMessage>> GetMessagesAsync()
    {
        var result = new List<ChatMessage>();

        if (_options.UseSystemPrompt)
        {
            result.Add(new SystemChatMessage(Constants.Strategy.BaseSystemPromptMessage));
        }

        return Task.FromResult(result);
    }
}
