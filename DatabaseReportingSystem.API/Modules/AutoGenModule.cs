using DatabaseReportingSystem.Features.AutoGen;

namespace DatabaseReportingSystem.Modules;

public static class AutoGenModule
{
    public static IEndpointRouteBuilder MapAutoGenModule(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("autogen");

        group.MapPost("analyze", AnalyzeFeatureEndpoint.AnalyzeAsync);

        return endpoints;
    }
}
