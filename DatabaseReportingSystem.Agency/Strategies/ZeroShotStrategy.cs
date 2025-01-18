using DatabaseReportingSystem.Shared;
using OpenAI.Chat;

namespace DatabaseReportingSystem.Agency.Strategies;

public sealed class ZeroShotStrategy(ZeroShotStrategy.Options options) : IStrategy
{
    private readonly Options _options = options;

    public bool OnlySystemPrompt { get; set; } = false;

    public Task<List<ChatMessage>> GetMessagesAsync()
    {
        var messages = new List<ChatMessage>();

        if (_options.UseSystemPrompt) messages.Add(new SystemChatMessage(Constants.Strategy.BaseSystemPromptMessage));

        return Task.FromResult(messages);
    }

    public sealed class Options
    {
        public bool UseSystemPrompt { get; init; } = true;
    }
}
