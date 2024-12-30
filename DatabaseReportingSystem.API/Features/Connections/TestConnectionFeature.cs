using DatabaseReportingSystem.Context;
using DatabaseReportingSystem.Shared;
using DatabaseReportingSystem.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DatabaseReportingSystem.Features.Connections;

public static class TestConnectionFeature
{
    public static async Task<IResult> TestConnectionAsync(
        [FromServices] SystemDbContext systemDbContext,
        [FromServices] IEncryptor encryptor,
        [FromBody] TestConnectionRequest request)
    {
        DatabaseManagementSystem databaseManagementSystem = request.DatabaseManagementSystem;
        ConnectionCredentialsDto? credentials = request.Credentials;

        if (request.Credentials is null)
        {
            User? user = await systemDbContext.Users.FirstOrDefaultAsync(u => u.Id == Constants.DefaultUserId);

            if (user is null)
            {
                return Results.NotFound("User not found.");
            }

            ConnectionCredentials connectionCredentials = user.ConnectionCredentials;

            databaseManagementSystem = connectionCredentials.DatabaseManagementSystem;

            string decryptedConnectionHash = encryptor.Decrypt(connectionCredentials.ConnectionHash);

            try
            {
                credentials = JsonConvert.DeserializeObject<ConnectionCredentialsDto>(decryptedConnectionHash);
            }
            catch (Exception)
            {
                return Results.BadRequest("Could not deserialize current connection credentials.");
            }
        }

        if (credentials is null)
        {
            return Results.BadRequest("Could not access connection credentials.");
        }

        string connectionString = Utilities.GenerateConnectionString(
            databaseManagementSystem,
            credentials);

        Result testResult = await Utilities.TestDatabaseConnectionAsync(
            databaseManagementSystem,
            connectionString);

        return testResult.IsSuccess
            ? Results.Ok()
            : Results.BadRequest(testResult.Error);
    }
}

public sealed record TestConnectionRequest(
    DatabaseManagementSystem DatabaseManagementSystem = DatabaseManagementSystem.Other,
    ConnectionCredentialsDto? Credentials = null);
