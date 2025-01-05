using DatabaseReportingSystem.Context;
using DatabaseReportingSystem.Shared;
using DatabaseReportingSystem.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatabaseReportingSystem.Features.Connections;

public static class TestConnectionFeature
{
    public static async Task<IResult> TestConnectionAsync(
        [FromServices] SystemDbContext systemDbContext,
        [FromServices] IEncryptor encryptor,
        [FromBody] TestConnectionRequest request)
    {
        DatabaseManagementSystem databaseManagementSystem;
        ConnectionCredentialsDto? credentials = request.Credentials;

        if (request.Credentials is null)
        {
            User? user = await systemDbContext.Users.FirstOrDefaultAsync(u => u.Id == Constants.DefaultUserId);

            if (user is null) return Results.NotFound("User not found.");

            databaseManagementSystem = user.ConnectionCredentials.DatabaseManagementSystem;

            var credentialsResult = encryptor.DecryptConnectionCredentials(user);

            if (credentialsResult.IsFailure) return Results.BadRequest(credentialsResult.Error);

            credentials = credentialsResult.Value;
        }
        else
        {
            databaseManagementSystem = request.Credentials.Dbms;
        }

        if (credentials is null) return Results.BadRequest("Could not access connection credentials.");

        string connectionString = Utilities.GenerateConnectionString(credentials);

        Result testResult = await Utilities.TestDatabaseConnectionAsync(
            databaseManagementSystem,
            connectionString);

        return testResult.IsSuccess
            ? Results.Ok()
            : Results.BadRequest(testResult.Error);
    }
}

public sealed record TestConnectionRequest(ConnectionCredentialsDto? Credentials = null);
