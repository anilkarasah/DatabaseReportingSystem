using DatabaseReportingSystem.Context;
using DatabaseReportingSystem.Shared;
using DatabaseReportingSystem.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DatabaseReportingSystem.Features;

public static class GetConnectionCredentialsFeature
{
    public static async Task<IResult> GetConnectionCredentialsAsync(
        [FromServices] IEncryptor encryptor,
        [FromServices] SystemDbContext systemDbContext)
    {
        User? user = await systemDbContext.Users.SingleOrDefaultAsync(u => u.Id == Constants.DefaultUserId);

        if (user is null)
        {
            return Results.NotFound("User not found.");
        }

        string credentialsHash = encryptor.Decrypt(user.ConnectionCredentials.ConnectionHash);

        ConnectionCredentialsDto? credentials = null;
        try
        {
            credentials = JsonConvert.DeserializeObject<ConnectionCredentialsDto>(credentialsHash);
        }
        catch (Exception e)
        {
            return Results.BadRequest("Could not deserialize connection credentials.");
        }

        return credentials is null
            ? Results.BadRequest("Could not get connection credentials.")
            : Results.Ok(credentials);
    }
}
