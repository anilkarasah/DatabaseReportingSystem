using DatabaseReportingSystem.Context;
using DatabaseReportingSystem.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatabaseReportingSystem.Features.Chats;

public static class SendMessageFeature
{
    public static async Task<IResult> SendMessageAsync(
        [FromServices] SystemDbContext systemDbContext,
        [FromRoute] Guid chatId,
        [FromBody] SendMessageRequest request)
    {
        Chat? chat = await systemDbContext.Chats
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == chatId);

        if (chat is null) return Results.NotFound("Chat not found.");

        var message = new ChatMessage
        {
            MessageId = Guid.NewGuid(),
            ChatId = chat.Id,
            Content = request.Content,
            Index = chat.Messages.Count,
            SentAtUtc = DateTime.UtcNow
        };

        systemDbContext.ChatMessages.Add(message);

        await systemDbContext.SaveChangesAsync();

        return Results.Ok(Shared.ChatMessageResponse.FromChatMessage(message));
    }
}

public sealed record SendMessageRequest(string Content);
