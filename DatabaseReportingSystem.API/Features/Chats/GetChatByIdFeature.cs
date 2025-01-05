using DatabaseReportingSystem.Context;
using DatabaseReportingSystem.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatabaseReportingSystem.Features.Chats;

public static class GetChatByIdFeature
{
    public static async Task<IResult> GetChatAsync(
        [FromServices] SystemDbContext systemDbContext,
        [FromRoute] Guid chatId)
    {
        Chat? chat = await systemDbContext.Chats
            .Include(c => c.Messages.OrderBy(m => m.Index))
            .ThenInclude(m => m.ModelResponses.OrderBy(r => r.ModelName))
            .FirstOrDefaultAsync(c => c.Id == chatId);

        return chat is null
            ? Results.NotFound("Chat not found.")
            : Results.Ok(Shared.ChatResponse.FromChat(chat));
    }
}
