using DatabaseReportingSystem.Agency.LanguageModels;
using DatabaseReportingSystem.Agency.Strategies;
using DatabaseReportingSystem.Shared;
using Microsoft.AspNetCore.Mvc;
using OpenAI.Chat;

namespace DatabaseReportingSystem.Features;

public static class AskGptWithZeroShotStrategyFeature
{
    public static async Task<IResult> ExecuteAsync(
        [FromServices] IConfiguration configuration,
        AskGptWithZeroShotStrategyRequest request)
    {
        var gptModel = new GptModel("gpt-4o-mini", configuration.GetConnectionString("GptApiKey")!);

        var zeroShotStrategy = new ZeroShotStrategy(new ZeroShotStrategy.Options());

        var messages = await zeroShotStrategy.GetMessagesAsync();

        UserChatMessage userMessage = Utilities.CreateUserChatMessage(
            request.Question,
            request.Schema,
            "dbms");

        messages.Add(userMessage);

        string response = await gptModel.AskAsync(messages);

        return Results.Ok(response);
    }
}

public sealed record AskGptWithZeroShotStrategyRequest(string Question, string Schema);
