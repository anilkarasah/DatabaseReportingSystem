using DatabaseReportingSystem.Context;
using DatabaseReportingSystem.Shared;
using DatabaseReportingSystem.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace DatabaseReportingSystem.Features.Chats;

public static class CreateChatFeature
{
    public static async Task<IResult> CreateChatAsync(
        [FromServices] SystemDbContext systemDbContext,
        [FromServices] IEncryptor encryptor,
        [FromBody] Request request)
    {
        string encryptedContent = encryptor.Encrypt(request.Schema);

        var chat = new Chat
        {
            Id = Guid.NewGuid(),
            UserId = Constants.DefaultUserId,
            DatabaseManagementSystem = request.DatabaseManagementSystem,
            SchemaHash = encryptedContent,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        systemDbContext.Chats.Add(chat);

        int savedItemCount = await systemDbContext.SaveChangesAsync();

        return savedItemCount > 0
            ? Results.Ok(Shared.ChatResponse.FromChat(chat))
            : Results.BadRequest("Could not create chat.");
    }

    public sealed record Request(DatabaseManagementSystem DatabaseManagementSystem, string Schema);
}
