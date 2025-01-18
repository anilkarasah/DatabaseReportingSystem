using AutoGen.Core;
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
        string modelName = largeLanguageModel.ToString().ToLower();

        ModelResponse? currentModelResponse = await systemDbContext.ModelResponses
            .FirstOrDefaultAsync(r => r.MessageId == messageId && r.ModelName == modelName);

        if (currentModelResponse is not null)
        {
            return Results.BadRequest("This message has already been processed by the LLM.");
        }

        ModelClient modelClient = modelClientFactory
            .GenerateModelClient(largeLanguageModel, strategy, clientOptions);

        var messages = await modelClient.Strategy.GetMessagesAsync();

        string schema = encryptor.Decrypt(chat.SchemaHash);

        UserChatMessage userMessage = Utilities
            .CreateUserChatMessage(new UserPromptDto(relatedMessage.Content, schema, chat.DatabaseManagementSystem));

        messages.Add(userMessage);

        string generatedQuery = await modelClient.LanguageModel.AskAsync(messages);

        if (string.IsNullOrWhiteSpace(generatedQuery))
        {
            generatedQuery = "SELECT 'Unable to generate a query.' AS message";
        }

        var modelResponse = new ModelResponse
        {
            MessageId = relatedMessage.MessageId,
            ModelName = modelName,
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
        string modelName = largeLanguageModel.ToString().ToLower();

        ModelResponse? currentModelResponse = await systemDbContext.ModelResponses
            .FirstOrDefaultAsync(r => r.MessageId == messageId && r.ModelName == modelName);

        if (currentModelResponse is not null)
        {
            return Results.BadRequest("This message has already been processed by the LLM.");
        }

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

        IMessage? lastWriterResponse = responses
            .LastOrDefault(r => r.From == "query-writer" && r.GetContent() != "TERMINATE");

        string modelResponseString = lastWriterResponse?.GetContent() ?? JsonConvert.SerializeObject(responses);

        var modelResponse = new ModelResponse
        {
            MessageId = relatedMessage.MessageId,
            ModelName = modelName,
            Content = modelResponseString,
            CompletedAtUtc = DateTimeOffset.UtcNow
        };

        systemDbContext.ModelResponses.Add(modelResponse);

        await systemDbContext.SaveChangesAsync();

        return Results.Ok(Shared.ModelResponseDto.FromModelResponse(modelResponse));
    }

    public static async Task<IResult> AskUsingManualGroupChatAsync(
        [FromServices] ManualGroupChatFeature feature,
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
        string modelName = largeLanguageModel.ToString().ToLower();

        ModelResponse? currentModelResponse = await systemDbContext.ModelResponses
            .FirstOrDefaultAsync(r => r.MessageId == messageId && r.ModelName == modelName);

        if (currentModelResponse is not null)
        {
            return Results.BadRequest("This message has already been processed by the LLM.");
        }

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

        (string query, var messages) = await feature.RunAsync(
            userQuestion,
            chat.DatabaseManagementSystem,
            modelClient.LanguageModel,
            modelClient.Strategy,
            credentials);

        var modelResponse = new ModelResponse
        {
            MessageId = relatedMessage.MessageId,
            ModelName = modelName,
            Content = query,
            CompletedAtUtc = DateTimeOffset.UtcNow
        };

        systemDbContext.ModelResponses.Add(modelResponse);

        await systemDbContext.SaveChangesAsync();

        return Results.Ok(Shared.ModelResponseDto.FromModelResponse(modelResponse));
    }
}

public sealed record AskRequest(string LargeLanguageModel, string StrategyType);
