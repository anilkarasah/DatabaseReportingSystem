using DatabaseReportingSystem.Vector.Context;
using DatabaseReportingSystem.Vector.Features;
using Microsoft.Extensions.DependencyInjection;

namespace DatabaseReportingSystem.Vector.Shared;

public static class DependencyInjection
{
    public static IServiceCollection AddVector(this IServiceCollection services)
    {
        return services
            .AddCreateEmbeddingFeature()
            .AddGetNearestQuestionsFeature()
            .AddGetRandomQuestionsFeature();
    }
}
