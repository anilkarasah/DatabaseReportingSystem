using Microsoft.Extensions.DependencyInjection;

namespace DatabaseReportingSystem.Agency.Shared;

public static class DependencyInjection
{
    public static IServiceCollection AddAgency(this IServiceCollection services)
        => services
            .AddTransient<ModelClientFactory>();
}
