using DatabaseReportingSystem.Agency.Features;
using DatabaseReportingSystem.Agency.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace DatabaseReportingSystem.Agency;

public static class DependencyInjection
{
    public static IServiceCollection AddAgency(this IServiceCollection services)
    {
        return services
            .AddTransient<AutoGenFeature>()
            .AddScoped<ManualGroupChatFeature>()
            .AddTransient<ModelClientFactory>();
    }
}
