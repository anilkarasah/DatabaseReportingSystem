using DatabaseReportingSystem.Shared;
using DatabaseReportingSystem.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace DatabaseReportingSystem.Features;

public static class SqlSyntaxValidatorFeature
{
    public static IResult ValidateSqlSyntax([FromBody] ValidateSqlCommand command)
    {
        var validateResult = Utilities.ValidateSqlQuerySyntax(command.DatabaseManagementSystem, command.Query);

        return validateResult.IsFailure
            ? Results.BadRequest(validateResult.Error)
            : Results.Ok(validateResult.Value);
    }
}

public sealed record ValidateSqlCommand(DatabaseManagementSystem DatabaseManagementSystem, string Query);
