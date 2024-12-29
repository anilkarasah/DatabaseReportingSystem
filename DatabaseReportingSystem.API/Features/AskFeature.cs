using DatabaseReportingSystem.Agency.Shared;
using DatabaseReportingSystem.Context;
using DatabaseReportingSystem.Shared;
using Microsoft.AspNetCore.Mvc;
using OpenAI.Chat;

namespace DatabaseReportingSystem.Features;

public static class AskFeature
{
    public static async Task<IResult> AskAsync(
        [FromServices] IConfiguration configuration,
        [FromServices] IServiceProvider serviceProvider,
        [FromServices] ModelClientFactory modelClientFactory,
        [FromServices] SystemDbContext systemDbContext,
        [FromQuery] string model,
        [FromQuery] string strategy,
        [FromBody] AskRequest request)
    {
        var clientOptions = new ClientOptions
        {
            NumberOfExamples = request.NumberOfExamples,
            UseSystemPrompt = request.UseSystemPrompt,
            Question = request.Question,
        };

        ModelClient modelClient = modelClientFactory.GenerateModelClient(model, strategy, clientOptions);

        var messages = await modelClient.Strategy.GetMessagesAsync();

        UserChatMessage userMessage = Utilities.CreateUserChatMessage(
            request.Question,
            request.Schema,
            "dbms");

        messages.Add(userMessage);

        string response = await modelClient.LanguageModel.AskAsync(messages);

        return Results.Ok(response);
    }
}

public sealed record AskRequest(
    string Question,
    string Schema,
    int NumberOfExamples = Constants.Strategy.DefaultNumberOfExamples,
    bool UseSystemPrompt = true);
