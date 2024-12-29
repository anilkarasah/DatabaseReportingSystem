using DatabaseReportingSystem.Agency.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace DatabaseReportingSystem.Agency;

public static class DependencyInjection
{
    public static IServiceCollection AddAgency(this IServiceCollection services)
        => services
            .AddTransient<ModelClientFactory>();
}
