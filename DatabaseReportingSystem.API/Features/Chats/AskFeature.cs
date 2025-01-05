using DatabaseReportingSystem.Agency.Features;
using DatabaseReportingSystem.Agency.Shared;
using DatabaseReportingSystem.Context;
using DatabaseReportingSystem.Shared;
using DatabaseReportingSystem.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OpenAI.Chat;
using ChatMessage = DatabaseReportingSystem.Shared.Models.ChatMessage;

namespace DatabaseReportingSystem.Features.Chats;

public static class AskFeature
{
    public static async Task<IResult> AskAsync(
        [FromServices] IServiceProvider serviceProvider,
        [FromServices] ModelClientFactory modelClientFactory,
        [FromServices] SystemDbContext systemDbContext,
        [FromServices] IEncryptor encryptor,
        [FromRoute] Guid chatId,
        [FromRoute] Guid messageId,
        [FromBody] AskRequest request)
    {
        Chat? chat = await systemDbContext.Chats
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == chatId);

        if (chat is null) return Results.NotFound("Chat not found.");

        ChatMessage? relatedMessage = chat.Messages.FirstOrDefault(m => m.MessageId == messageId);

        if (relatedMessage is null) return Results.NotFound("Message not found.");

        var clientOptions = new ClientOptions
        {
            Question = relatedMessage.Content
        };

        LargeLanguageModel largeLanguageModel = request.LargeLanguageModel.AsLargeLanguageModel();
        StrategyType strategy = request.StrategyType.AsStrategyType();

        ModelClient modelClient = modelClientFactory
            .GenerateModelClient(largeLanguageModel, strategy, clientOptions);

        string systemPrompt = await modelClient.Strategy.GetMessagesAsync();

        List<OpenAI.Chat.ChatMessage> messages = [new SystemChatMessage(systemPrompt)];

        string schema = encryptor.Decrypt(chat.SchemaHash);

        UserChatMessage userMessage = Utilities
            .CreateUserChatMessage(new UserPromptDto(relatedMessage.Content, schema, chat.DatabaseManagementSystem));

        messages.Add(userMessage);

        string generatedQuery = await modelClient.LanguageModel.AskAsync(messages);

        var modelResponse = new ModelResponse
        {
            MessageId = relatedMessage.MessageId,
            ModelName = largeLanguageModel.ToString().ToLower(),
            Content = generatedQuery,
            CompletedAtUtc = DateTimeOffset.UtcNow
        };

        systemDbContext.ModelResponses.Add(modelResponse);

        await systemDbContext.SaveChangesAsync();

        return Results.Ok(Shared.ModelResponseDto.FromModelResponse(modelResponse));
    }

    public static async Task<IResult> AskUsingAutoGenAsync(
        [FromServices] AutoGenFeature feature,
        [FromServices] SystemDbContext systemDbContext,
        [FromServices] ModelClientFactory modelClientFactory,
        [FromServices] IEncryptor encryptor,
        [FromRoute] Guid chatId,
        [FromRoute] Guid messageId,
        [FromBody] AskRequest request)
    {
        Chat? chat = await systemDbContext.Chats
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == chatId);

        if (chat is null) return Results.NotFound("Chat not found.");

        ChatMessage? relatedMessage = chat.Messages.FirstOrDefault(m => m.MessageId == messageId);

        if (relatedMessage is null) return Results.NotFound("Message not found.");

        LargeLanguageModel largeLanguageModel = request.LargeLanguageModel.AsLargeLanguageModel();
        StrategyType strategy = request.StrategyType.AsStrategyType();

        var clientOptions = new ClientOptions
        {
            Question = relatedMessage.Content
        };

        ModelClient modelClient = modelClientFactory
            .GenerateModelClient(largeLanguageModel, strategy, clientOptions);

        string schema = encryptor.Decrypt(chat.SchemaHash);

        var userQuestion = new UserPromptDto(relatedMessage.Content, schema, chat.DatabaseManagementSystem);

        User? user = await systemDbContext.Users.FirstOrDefaultAsync(u => u.Id == Constants.DefaultUserId);

        if (user is null) return Results.BadRequest("User not found.");

        var credentialsResult = encryptor.DecryptConnectionCredentials(user);

        ConnectionCredentialsDto? credentials = credentialsResult.IsSuccess
            ? credentialsResult.Value
            : null;

        var responses = (await feature.RunGroupChatAsync(
            userQuestion,
            chat.DatabaseManagementSystem,
            modelClient.LanguageModel,
            modelClient.Strategy,
            credentials)).ToList();

        string autoGenResponsesJson = JsonConvert.SerializeObject(responses);

        var modelResponse = new ModelResponse
        {
            MessageId = relatedMessage.MessageId,
            ModelName = largeLanguageModel.ToString().ToLower(),
            Content = autoGenResponsesJson,
            CompletedAtUtc = DateTimeOffset.UtcNow
        };

        systemDbContext.ModelResponses.Add(modelResponse);

        await systemDbContext.SaveChangesAsync();

        return Results.Ok(Shared.ModelResponseDto.FromModelResponse(modelResponse));
    }
}

public sealed record AskRequest(string LargeLanguageModel, string StrategyType);
