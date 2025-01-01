using System.Text;
using DatabaseReportingSystem.Shared;

namespace DatabaseReportingSystem.Agency.Strategies;

public sealed class ZeroShotStrategy(ZeroShotStrategy.Options options) : IStrategy
{
    private readonly Options _options = options;

    public Task<string> GetMessagesAsync()
    {
        var stringBuilder = new StringBuilder();

        if (_options.UseSystemPrompt) stringBuilder.AppendLine(Constants.Strategy.BaseSystemPromptMessage);

        return Task.FromResult(stringBuilder.ToString());
    }

    public sealed class Options
    {
        public bool UseSystemPrompt { get; init; } = true;
    }
}
