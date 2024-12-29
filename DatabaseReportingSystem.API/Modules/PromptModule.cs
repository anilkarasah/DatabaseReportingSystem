using DatabaseReportingSystem.Features.Connections;

namespace DatabaseReportingSystem.Modules;

public static class PromptModule
{
    public static IEndpointRouteBuilder MapConnectionCredentialsModule(this IEndpointRouteBuilder builder)
    {
        RouteGroupBuilder group = builder.MapGroup("connection-credentials");

        group.MapPost("/", EditConnectionCredentialsFeature.EditConnectionCredentialsAsync);

        group.MapGet("/", GetConnectionCredentialsFeature.GetConnectionCredentialsAsync);

        return builder;
    }
}
