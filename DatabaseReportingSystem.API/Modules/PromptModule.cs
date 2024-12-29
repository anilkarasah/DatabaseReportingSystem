using DatabaseReportingSystem.Features;

namespace DatabaseReportingSystem.Modules;

public static class PromptModule
{
    public static IEndpointRouteBuilder MapPromptModule(this IEndpointRouteBuilder builder)
    {
        RouteGroupBuilder group = builder.MapGroup("ask");

        group.MapPost("/", AskFeature.AskAsync);

        return builder;
    }

    public static IEndpointRouteBuilder MapConnectionCredentialsModule(this IEndpointRouteBuilder builder)
    {
        RouteGroupBuilder group = builder.MapGroup("connection-credentials");

        group.MapPost("/", EditConnectionCredentialsFeature.EditConnectionCredentialsAsync);

        group.MapGet("/", GetConnectionCredentialsFeature.GetConnectionCredentialsAsync);

        return builder;
    }
}
