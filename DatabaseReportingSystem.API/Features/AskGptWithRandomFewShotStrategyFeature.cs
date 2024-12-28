using DatabaseReportingSystem.Agency.LanguageModels;
using DatabaseReportingSystem.Agency.Strategies;
using DatabaseReportingSystem.Shared;
using Microsoft.AspNetCore.Mvc;
using OpenAI.Chat;

namespace DatabaseReportingSystem.Features;

public static class AskGptWithRandomFewShotStrategyFeature
{
    public static async Task<IResult> ExecuteAsync(
        [FromServices] IConfiguration configuration,
        [FromServices] IServiceProvider serviceProvider,
        [FromBody] AskGptWithRandomFewShotStrategyRequest request)
    {
        var gptModel = new GptModel("gpt-4o-mini", configuration.GetConnectionString("GptApiKey")!);

        var randomFewShotStrategy = new RandomFewShotStrategy(new RandomFewShotStrategy.Options
        {
            ServiceProvider = serviceProvider,
            NumberOfExamples = request.NumberOfExamples,
            UseSystemPrompt = request.UseSystemPrompt,
        });

        var messages = await randomFewShotStrategy.GetMessagesAsync();
        
        UserChatMessage userMessage = Utilities.CreateUserChatMessage(
            request.Question,
            request.Schema,
            "dbms");
        
        messages.Add(userMessage);

        string response = await gptModel.AskAsync(messages);

        return Results.Ok(response);
    }
}

public record AskGptWithRandomFewShotStrategyRequest(
    string Question,
    string Schema,
    int NumberOfExamples = Constants.Strategy.DefaultNumberOfExamples,
    bool UseSystemPrompt = true);
