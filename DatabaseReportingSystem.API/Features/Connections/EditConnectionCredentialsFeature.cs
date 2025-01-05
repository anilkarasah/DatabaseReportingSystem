using DatabaseReportingSystem.Context;
using DatabaseReportingSystem.Shared;
using DatabaseReportingSystem.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DatabaseReportingSystem.Features.Connections;

public static class EditConnectionCredentialsFeature
{
    public static async Task<IResult> EditConnectionCredentialsAsync(
        [FromServices] IEncryptor encryptor,
        [FromServices] SystemDbContext systemDbContext,
        [FromBody] ConnectionCredentialsDto request)
    {
        User? user = await systemDbContext.Users.SingleOrDefaultAsync(u => u.Id == Constants.DefaultUserId);

        if (user is null) return Results.NotFound("User not found.");

        string requestJson = JsonConvert.SerializeObject(request, Formatting.None);

        string connectionHash = encryptor.Encrypt(requestJson);

        user.ConnectionCredentials = new ConnectionCredentials
        {
            DatabaseManagementSystem = request.Dbms,
            ConnectionHash = connectionHash
        };

        await systemDbContext.SaveChangesAsync();

        return Results.Ok();
    }
}
