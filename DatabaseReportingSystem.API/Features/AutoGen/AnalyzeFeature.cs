using DatabaseReportingSystem.Context;
using DatabaseReportingSystem.Shared;
using DatabaseReportingSystem.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DatabaseReportingSystem.Features.AutoGen;

public sealed class AnalyzeFeature(IConfiguration configuration, SystemDbContext systemDbContext, IEncryptor encryptor)
{
    private readonly IConfiguration _configuration = configuration;
    private readonly SystemDbContext _systemDbContext = systemDbContext;
    private readonly IEncryptor _encryptor = encryptor;

    public async Task<Result<string>> AnalyzeAsync(string query)
    {
        User? user = await _systemDbContext.Users.FirstOrDefaultAsync(u => u.Id == Constants.DefaultUserId);
        
        if (user is null)
        {
            return Result<string>.Fail("User not found");
        }
        
        var credentialsResult = _encryptor.DecryptConnectionCredentials(user);
        
        if (credentialsResult.IsFailure)
        {
            return Result<string>.Fail(credentialsResult.Error);
        }
        
        ConnectionCredentialsDto credentials = credentialsResult.Value;
        DatabaseManagementSystem databaseManagementSystem = user.ConnectionCredentials.DatabaseManagementSystem;

        string connectionString = Utilities.GenerateConnectionString(databaseManagementSystem, credentials);
        
        var queryResult = await Utilities.QueryOnUserDatabaseAsync(databaseManagementSystem, connectionString, query);
        
        if (queryResult.IsFailure)
        {
            return Result<string>.Fail(queryResult.Error);
        }

        string queryResultJson = JsonConvert.SerializeObject(queryResult.Value.Values);
        
        return await Task.FromResult(Result<string>.Ok(query));
    }
}

public static class AnalyzeFeatureEndpoint
{
    public static async Task<IResult> AnalyzeAsync([FromServices] AnalyzeFeature feature, [FromBody] string query)
    {
        var analyzeResult = await feature.AnalyzeAsync(query);

        return analyzeResult.IsSuccess
            ? Results.Ok(analyzeResult.Value)
            : Results.BadRequest(analyzeResult.Error);
    }
}
