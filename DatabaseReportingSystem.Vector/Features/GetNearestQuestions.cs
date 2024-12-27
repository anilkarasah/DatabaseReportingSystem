using DatabaseReportingSystem.Vector.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pgvector.EntityFrameworkCore;

namespace DatabaseReportingSystem.Vector.Features;

public static class GetNearestQuestions
{
    public static IServiceCollection AddGetNearestQuestionsFeature(this IServiceCollection services)
        => services.AddScoped<Feature>();

    public sealed class Feature(VectorDbContext dbContext)
    {
        private readonly VectorDbContext _dbContext = dbContext;

        public async Task<List<NearestQuestionDto>> GetNearestQuestionsAsync(Request request)
        {
            float[] exampleEmbeddings = [ /* 1536 elements, representing an embedding */ ];

            var nearestQuestions = await _dbContext.Embeddings
                .Include(e => e.Schema)
                .OrderBy(e => e.Embedding.L2Distance(new Pgvector.Vector(exampleEmbeddings)))
                .Take(5)
                .Select(e => new NearestQuestionDto(e.Question, e.Schema.Schema, e.Query))
                .ToListAsync();

            return nearestQuestions;
        }
    }

    public sealed record Request(string Question);

    public sealed record NearestQuestionDto(string Question, string Schema, string Query);
}
