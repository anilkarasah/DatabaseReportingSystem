using DatabaseReportingSystem.Context;
using DatabaseReportingSystem.Shared;
using DatabaseReportingSystem.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatabaseReportingSystem.Features.Connections;

public static class QueryFeature
{
    public static async Task<IResult> QueryAsync(
        [FromServices] SystemDbContext systemDbContext,
        [FromServices] IEncryptor encryptor,
        [FromBody] QueryRequest request)
    {
        User? user = await systemDbContext.Users.FirstOrDefaultAsync(u => u.Id == Constants.DefaultUserId);

        if (user is null) return Results.NotFound("User not found.");

        ConnectionCredentials connectionCredentials = user.ConnectionCredentials;

        var credentialsResult = encryptor.DecryptConnectionCredentials(user);

        if (credentialsResult.IsFailure) return Results.BadRequest(credentialsResult.Error);

        ConnectionCredentialsDto credentials = credentialsResult.Value;

        string connectionString = Utilities.GenerateConnectionString(credentials);

        var queryResult = await Utilities.QueryOnUserDatabaseAsync(
            connectionCredentials.DatabaseManagementSystem,
            connectionString,
            request.Query);

        return queryResult.IsSuccess
            ? Results.Ok(queryResult.Value)
            : Results.BadRequest(queryResult.Error);
    }
}

public sealed record QueryRequest(string Query);
