using DatabaseReportingSystem.Context;
using DatabaseReportingSystem.Shared;
using DatabaseReportingSystem.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DatabaseReportingSystem.Features.Connections;

public static class QueryFeature
{
    public static async Task<IResult> QueryAsync(
        [FromServices] SystemDbContext systemDbContext,
        [FromServices] IEncryptor encryptor,
        [FromBody] QueryRequest request)
    {
        User? user = await systemDbContext.Users.FirstOrDefaultAsync(u => u.Id == Constants.DefaultUserId);
        
        if (user is null)
        {
            return Results.NotFound("User not found.");
        }
        
        ConnectionCredentials connectionCredentials = user.ConnectionCredentials;
        
        string decryptedConnectionHash = encryptor.Decrypt(connectionCredentials.ConnectionHash);
        
        ConnectionCredentialsDto? credentials = null;
        try
        {
            credentials = JsonConvert.DeserializeObject<ConnectionCredentialsDto>(decryptedConnectionHash);
        }
        catch (Exception)
        {
            return Results.BadRequest("Could not deserialize current connection credentials.");
        }
        
        if (credentials is null)
        {
            return Results.BadRequest("Could not access connection credentials.");
        }
        
        string connectionString = Utilities.GenerateConnectionString(
            connectionCredentials.DatabaseManagementSystem,
            credentials);
        
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
