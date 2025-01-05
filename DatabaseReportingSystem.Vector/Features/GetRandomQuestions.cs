using DatabaseReportingSystem.Shared;
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

        public async Task<List<GetRandomQuestionsResponse>> GetRandomQuestionsAsync(
            GetRandomQuestionsRequest getRandomQuestionsRequest)
        {
            var randomQuestions = await _vectorDbContext.Embeddings
                .Include(e => e.Schema)
                .OrderBy(_ => Guid.NewGuid())
                .Take(getRandomQuestionsRequest.NumberOfQuestions)
                .Select(e => new GetRandomQuestionsResponse(e.Question, e.Schema.Schema, e.Query))
                .ToListAsync();

            return randomQuestions;
        }
    }

    public sealed record GetRandomQuestionsRequest(int NumberOfQuestions = Constants.Strategy.DefaultNumberOfExamples);

    public sealed record GetRandomQuestionsResponse(string Question, string Schema, string Query);
}
