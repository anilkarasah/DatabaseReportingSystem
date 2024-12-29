using DatabaseReportingSystem.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatabaseReportingSystem.Features.Chats;

public static class GetChatsFeature
{
    public static async Task<IResult> GetChatsAsync([FromServices] SystemDbContext systemDbContext)
    {
        var chats = await systemDbContext.Chats.ToListAsync();

        return Results.Ok(chats);
    }
}
