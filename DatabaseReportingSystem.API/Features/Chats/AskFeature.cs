using DatabaseReportingSystem.Agency.Shared;
using DatabaseReportingSystem.Context;
using DatabaseReportingSystem.Shared;
using DatabaseReportingSystem.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenAI.Chat;
using ChatMessage = DatabaseReportingSystem.Shared.Models.ChatMessage;

namespace DatabaseReportingSystem.Features.Chats;

public static class AskFeature
{
    public static async Task<IResult> AskAsync(
        [FromServices] IConfiguration configuration,
        [FromServices] IServiceProvider serviceProvider,
        [FromServices] ModelClientFactory modelClientFactory,
        [FromServices] SystemDbContext systemDbContext,
        [FromServices] IEncryptor encryptor,
        [FromRoute] Guid chatId,
        [FromBody] AskRequest request)
    {
        if (request.ReplyingToMessageId == Guid.Empty)
        {
            return Results.BadRequest("Replying to message ID cannot be empty.");
        }

        Chat? chat = await systemDbContext.Chats
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == chatId);

        if (chat is null)
        {
            return Results.NotFound("Chat not found.");
        }

        ChatMessage? relatedMessage = chat.Messages.FirstOrDefault(m => m.MessageId == request.ReplyingToMessageId);

        if (relatedMessage is null)
        {
            return Results.NotFound("Message not found.");
        }

        var clientOptions = new ClientOptions
        {
            Question = relatedMessage.Content,
        };

        LargeLanguageModel largeLanguageModel = request.ModelName.AsLargeLanguageModel();
        StrategyType strategy = request.Strategy.AsStrategyType();

        ModelClient modelClient = modelClientFactory
            .GenerateModelClient(largeLanguageModel, strategy, clientOptions);

        var messages = await modelClient.Strategy.GetMessagesAsync();

        string schema = encryptor.Decrypt(chat.SchemaHash);

        UserChatMessage userMessage = Utilities.CreateUserChatMessage(
            relatedMessage.Content,
            schema,
            chat.DatabaseManagementSystem);

        messages.Add(userMessage);

        string generatedQuery = await modelClient.LanguageModel.AskAsync(messages);

        var modelResponse = new ModelResponse
        {
            MessageId = relatedMessage.MessageId,
            ModelName = largeLanguageModel.ToString().ToLower(),
            Content = generatedQuery,
            CompletedAtUtc = DateTimeOffset.UtcNow,
        };

        systemDbContext.ModelResponses.Add(modelResponse);

        await systemDbContext.SaveChangesAsync();

        return Results.Ok(modelResponse);
    }
}

public sealed record AskRequest(Guid ReplyingToMessageId, string ModelName, string Strategy);
