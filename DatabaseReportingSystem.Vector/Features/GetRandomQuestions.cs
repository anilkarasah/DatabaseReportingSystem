using DatabaseReportingSystem.Vector.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DatabaseReportingSystem.Vector.Features;

public static class GetRandomQuestions
{
    public static IServiceCollection AddGetRandomQuestionsFeature(this IServiceCollection services)
    {
        return services.AddScoped<Feature>();
    }

    public sealed class Feature(VectorDbContext vectorDbContext)
    {
        private readonly VectorDbContext _vectorDbContext = vectorDbContext;

        public async Task<List<Response>> GetRandomQuestionsAsync(Request request)
        {
            var randomQuestions = await _vectorDbContext.Embeddings
                .Include(e => e.Schema)
                .OrderBy(_ => Guid.NewGuid())
                .Take(request.NumberOfQuestions)
                .Select(e => new Response(e.Question, e.Schema.Schema, e.Query))
                .ToListAsync();

            return randomQuestions;
        }
    }

    public sealed record Request(int NumberOfQuestions);

    public sealed record Response(string Question, string Schema, string Query);
}
