using DatabaseReportingSystem.Agency.LanguageModels;
using DatabaseReportingSystem.Agency.Strategies;
using DatabaseReportingSystem.Shared;
using DatabaseReportingSystem.Shared.Models;
using DatabaseReportingSystem.Shared.Settings;
using Microsoft.Extensions.Options;

namespace DatabaseReportingSystem.Agency.Shared;

public sealed class ModelClientFactory(IOptions<ApiKeys> apiKeys, IServiceProvider serviceProvider)
{
    private readonly ApiKeys _apiKeys = apiKeys.Value;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public ModelClient GenerateModelClient(string modelName, string strategyName, ClientOptions options)
    {
        LargeLanguageModel languageModel = Utilities.GetLargeLanguageModel(modelName);
        StrategyType strategyType = Utilities.GetStrategyType(strategyName);

        return GenerateClient(languageModel, strategyType, options);
    }

    public ModelClient GenerateModelClient(
        LargeLanguageModel largeLanguageModel,
        StrategyType strategyType,
        ClientOptions options)
    {
        return GenerateClient(largeLanguageModel, strategyType, options);
    }

    private ModelClient GenerateClient(
        LargeLanguageModel largeLanguageModel,
        StrategyType strategyType,
        ClientOptions options)
    {
        ILanguageModel languageModel = largeLanguageModel switch
        {
            LargeLanguageModel.GPT => new GptModel("gpt-4o-mini", _apiKeys.GptApiKey),
            LargeLanguageModel.Grok => new GrokModel("grok-beta", _apiKeys.GrokApiKey),
            LargeLanguageModel.Mistral => new MistralModel(),
            LargeLanguageModel.CodeLLaMa => new CodeLlamaModel(),
            _ => throw new ArgumentException("Invalid language model.")
        };

        IStrategy strategy = strategyType switch
        {
            StrategyType.ZeroShot => new ZeroShotStrategy(new ZeroShotStrategy.Options
            {
                UseSystemPrompt = options.UseSystemPrompt
            }),
            StrategyType.RandomFewShot => new RandomFewShotStrategy(new RandomFewShotStrategy.Options
            {
                ServiceProvider = _serviceProvider,
                NumberOfExamples = options.NumberOfExamples,
                UseSystemPrompt = options.UseSystemPrompt
            }),
            StrategyType.NearestFewShot => new NearestFewShotStrategy(new NearestFewShotStrategy.Options
            {
                ServiceProvider = _serviceProvider,
                Question = options.Question ?? throw new ArgumentNullException(nameof(options.Question)),
                NumberOfExamples = options.NumberOfExamples,
                UseSystemPrompt = options.UseSystemPrompt
            }),
            StrategyType.DailSql => throw new NotImplementedException("DAIL-SQL method is not yet implemented."),
            _ => throw new ArgumentException("Invalid strategy.")
        };

        return new ModelClient(languageModel, strategy);
    }
}

public sealed record ClientOptions
{
    public bool UseSystemPrompt { get; } = true;

    public int NumberOfExamples { get; } = Constants.Strategy.DefaultNumberOfExamples;

    public string Question { get; init; } = string.Empty;
}

public sealed record ModelClient(ILanguageModel LanguageModel, IStrategy Strategy);
