using DatabaseReportingSystem.Context;
using DatabaseReportingSystem.Shared;
using DatabaseReportingSystem.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DatabaseReportingSystem.Features.Connections;

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

        var credentialsResult = encryptor.DecryptConnectionCredentials(user);

        return credentialsResult.IsFailure
            ? Results.BadRequest(credentialsResult.Error)
            : Results.Ok(credentialsResult.Value);
    }
}
