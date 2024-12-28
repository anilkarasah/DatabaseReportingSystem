using DatabaseReportingSystem.Agency.LanguageModels;
using DatabaseReportingSystem.Agency.Strategies;
using DatabaseReportingSystem.Shared;
using DatabaseReportingSystem.Shared.Models;
using Microsoft.Extensions.Configuration;

namespace DatabaseReportingSystem.Agency.Shared;

public sealed class ModelClientFactory(IConfiguration configuration, IServiceProvider serviceProvider)
{
    private readonly IConfiguration _configuration = configuration;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public LargeLanguageModel GetLargeLanguageModel(string modelName)
        => modelName switch
        {
            "gpt-4o-mini" => LargeLanguageModel.GPT,
            "gpt-4o" => LargeLanguageModel.GPT,
            "gpt" => LargeLanguageModel.GPT,
            "grok-beta" => LargeLanguageModel.Grok,
            "grok" => LargeLanguageModel.Grok,
            "mistral" => LargeLanguageModel.Mistral,
            "codellama" => LargeLanguageModel.CodeLLaMa,
            "llama" => LargeLanguageModel.CodeLLaMa,
            _ => throw new ArgumentException("Invalid model name.")
        };

    public StrategyType GetStrategyType(string strategyName)
        => strategyName switch
        {
            "basic" => StrategyType.ZeroShot,
            "zero" => StrategyType.ZeroShot,
            "zero-shot" => StrategyType.ZeroShot,
            "random" => StrategyType.RandomFewShot,
            "random-shot" => StrategyType.RandomFewShot,
            "random-few-shot" => StrategyType.RandomFewShot,
            "nearest" => StrategyType.NearestFewShot,
            "nearest-shot" => StrategyType.NearestFewShot,
            "nearest-few-shot" => StrategyType.NearestFewShot,
            _ => throw new ArgumentException("Invalid strategy name.")
        };

    public ModelClient GenerateModelClient(string modelName, string strategyName, ClientOptions options)
    {
        LargeLanguageModel languageModel = GetLargeLanguageModel(modelName);
        StrategyType strategyType = GetStrategyType(strategyName);

        return GenerateClient(languageModel, strategyType, options);
    }

    public ModelClient GenerateModelClient(
        LargeLanguageModel largeLanguageModel,
        StrategyType strategyType,
        ClientOptions options)
        => GenerateClient(largeLanguageModel, strategyType, options);

    private ModelClient GenerateClient(
        LargeLanguageModel largeLanguageModel,
        StrategyType strategyType,
        ClientOptions options)
    {
        ILanguageModel languageModel = largeLanguageModel switch
        {
            LargeLanguageModel.GPT => new GptModel("gpt-4o-mini", _configuration.GetConnectionString("GptApiKey")!),
            LargeLanguageModel.Grok => new GrokModel("grok-beta", _configuration.GetConnectionString("GrokApiKey")!),
            LargeLanguageModel.Mistral => new MistralModel(),
            LargeLanguageModel.CodeLLaMa => new CodeLlamaModel(),
            _ => throw new ArgumentException("Invalid language model.")
        };

        IStrategy strategy = strategyType switch
        {
            StrategyType.ZeroShot => new ZeroShotStrategy(new ZeroShotStrategy.Options
            {
                UseSystemPrompt = options.UseSystemPrompt,
            }),
            StrategyType.RandomFewShot => new RandomFewShotStrategy(new RandomFewShotStrategy.Options
            {
                ServiceProvider = _serviceProvider,
                NumberOfExamples = options.NumberOfExamples,
                UseSystemPrompt = options.UseSystemPrompt,
            }),
            StrategyType.NearestFewShot => new NearestFewShotStrategy(new NearestFewShotStrategy.Options
            {
                ServiceProvider = _serviceProvider,
                Question = options.Question ?? throw new ArgumentNullException(nameof(options.Question)),
                NumberOfExamples = options.NumberOfExamples,
                UseSystemPrompt = options.UseSystemPrompt,
            }),
            StrategyType.DailSql => throw new NotImplementedException("DAIL-SQL method is not yet implemented."),
            _ => throw new ArgumentException("Invalid strategy.")
        };

        return new ModelClient(languageModel, strategy);
    }
}

public sealed record ClientOptions
{
    public bool UseSystemPrompt { get; init; } = true;

    public int NumberOfExamples { get; init; } = Constants.Strategy.DefaultNumberOfExamples;

    public string Question { get; init; } = string.Empty;
}

public sealed record ModelClient(ILanguageModel LanguageModel, IStrategy Strategy);
