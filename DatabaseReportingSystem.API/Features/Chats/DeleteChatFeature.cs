using DatabaseReportingSystem.Context;
using DatabaseReportingSystem.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatabaseReportingSystem.Features.Chats;

public static class DeleteChatFeature
{
    public static async Task<IResult> DeleteChatAsync([FromRoute] Guid chatId, [FromServices] SystemDbContext context)
    {
        Chat? chat = await context.Chats.FirstOrDefaultAsync(c => c.Id == chatId);

        if (chat is null) return Results.NotFound();

        context.Chats.Remove(chat);
        await context.SaveChangesAsync();

        return Results.NoContent();
    }
}
