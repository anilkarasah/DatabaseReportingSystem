using DatabaseReportingSystem.Agency;
using DatabaseReportingSystem.Features.AutoGen;
using DatabaseReportingSystem.Modules;
using DatabaseReportingSystem.Shared;
using DatabaseReportingSystem.Vector;

namespace DatabaseReportingSystem;

public static class DependencyInjection
{
    public static IServiceCollection AddDatabaseReportingSystem(this IServiceCollection services)
    {
        return services
            .AddSingleton<IEncryptor, Encryptor>()
            .AddScoped<AnalyzeFeature>()
            .AddAgency()
            .AddVector();
    }

    public static IEndpointRouteBuilder MapDatabaseReportingSystem(this IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapConnectionCredentialsModule()
            .MapChatModule()
            .MapAutoGenModule()
            .MapVectorModule();
    }
}
