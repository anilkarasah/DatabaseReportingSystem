using DatabaseReportingSystem.Agency.LanguageModels;
using DatabaseReportingSystem.Agency.Strategies;
using DatabaseReportingSystem.Shared;
using Microsoft.AspNetCore.Mvc;
using OpenAI.Chat;

namespace DatabaseReportingSystem.Features;

public static class AskGptWithNearestFewShotStrategyFeature
{
    public static async Task<IResult> ExecuteAsync(
        [FromServices] IConfiguration configuration,
        [FromServices] IServiceProvider serviceProvider,
        [FromBody] AskGptWithNearestFewShotStrategyRequest request)
    {
        var gptModel = new GptModel("gpt-4o-mini", configuration.GetConnectionString("GptApiKey")!);

        var nearestFewShotStrategy = new NearestFewShotStrategy(new NearestFewShotStrategy.Options
        {
            ServiceProvider = serviceProvider,
            Question = request.Question,
            NumberOfExamples = request.NumberOfExamples,
            UseSystemPrompt = request.UseSystemPrompt,
        });

        var messages = await nearestFewShotStrategy.GetMessagesAsync();

        UserChatMessage userMessage = Utilities.CreateUserChatMessage(
            request.Question,
            request.Schema,
            "dbms");
        
        messages.Add(userMessage);

        string response = await gptModel.AskAsync(messages);

        return Results.Ok(response);
    }
}

public record AskGptWithNearestFewShotStrategyRequest(
    string Question,
    string Schema,
    int NumberOfExamples = Constants.Strategy.DefaultNumberOfExamples,
    bool UseSystemPrompt = true);
