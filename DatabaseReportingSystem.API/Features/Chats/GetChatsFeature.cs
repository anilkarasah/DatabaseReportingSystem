using DatabaseReportingSystem.Context;
using DatabaseReportingSystem.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatabaseReportingSystem.Features.Chats;

public static class GetChatsFeature
{
    public static async Task<IResult> GetChatsAsync([FromServices] SystemDbContext systemDbContext)
    {
        var chats = await systemDbContext.Chats
            .Include(c => c.Messages.OrderBy(m => m.Index))
            .ThenInclude(m => m.ModelResponses.OrderBy(r => r.ModelName))
            .Where(c => c.UserId == Constants.DefaultUserId)
            .Select(c => Shared.ChatResponse.FromChat(c))
            .ToListAsync();

        return Results.Ok(chats);
    }
}
