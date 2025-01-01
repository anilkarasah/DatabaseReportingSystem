namespace DatabaseReportingSystem.Modules;

public static class AutoGenModule
{
    public static IEndpointRouteBuilder MapAutoGenModule(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("autogen");

        return endpoints;
    }
}
