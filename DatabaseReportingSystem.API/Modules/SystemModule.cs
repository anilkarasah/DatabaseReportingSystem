using DatabaseReportingSystem.Features.Connections;

namespace DatabaseReportingSystem.Modules;

public static class SystemModule
{
    public static IEndpointRouteBuilder MapConnectionCredentialsModule(this IEndpointRouteBuilder builder)
    {
        RouteGroupBuilder group = builder.MapGroup("connection-credentials");

        group.MapPut("/", EditConnectionCredentialsFeature.EditConnectionCredentialsAsync);

        group.MapGet("/", GetConnectionCredentialsFeature.GetConnectionCredentialsAsync);

        group.MapPost("test", TestConnectionFeature.TestConnectionAsync);

        group.MapPost("execute", QueryFeature.QueryAsync);

        return builder;
    }
}
