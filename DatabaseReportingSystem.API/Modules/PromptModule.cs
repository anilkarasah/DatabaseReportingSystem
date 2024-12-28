using DatabaseReportingSystem.Features;

namespace DatabaseReportingSystem.Modules;

public static class PromptModule
{
    public static IEndpointRouteBuilder MapPromptModule(this IEndpointRouteBuilder builder)
    {
        RouteGroupBuilder group = builder.MapGroup("prompts");

        group.MapPost("gpt/zero-shot", AskGptWithZeroShotStrategyFeature.ExecuteAsync);

        group.MapPost("gpt/random-few-shot", AskGptWithRandomFewShotStrategyFeature.ExecuteAsync);

        group.MapPost("gpt/nearest-few-shot", AskGptWithNearestFewShotStrategyFeature.ExecuteAsync);

        return builder;
    }
}
