using DatabaseReportingSystem.Features;
using DatabaseReportingSystem.Features.Connections;

namespace DatabaseReportingSystem.Modules;

public static class SystemModule
{
    public static IEndpointRouteBuilder MapConnectionCredentialsModule(this IEndpointRouteBuilder builder)
    {
        RouteGroupBuilder connectionCredentialsGroup = builder.MapGroup("connection-credentials");

        connectionCredentialsGroup.MapPut("/", EditConnectionCredentialsFeature.EditConnectionCredentialsAsync);

        connectionCredentialsGroup.MapGet("/", GetConnectionCredentialsFeature.GetConnectionCredentialsAsync);

        connectionCredentialsGroup.MapPost("test", TestConnectionFeature.TestConnectionAsync);

        connectionCredentialsGroup.MapPost("execute", QueryFeature.QueryAsync);
        
        RouteGroupBuilder validatorsGroup = builder.MapGroup("validators");
        
        validatorsGroup.MapPost("sql", SqlSyntaxValidatorFeature.ValidateSqlSyntax);

        return builder;
    }
}
