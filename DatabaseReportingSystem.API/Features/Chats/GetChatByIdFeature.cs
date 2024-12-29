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
            .Include(c => c.Messages)
            .ThenInclude(m => m.ModelResponses)
            .FirstOrDefaultAsync(c => c.Id == chatId);

        return chat is null
            ? Results.NotFound("Chat not found.")
            : Results.Ok(chat);
    }
}
