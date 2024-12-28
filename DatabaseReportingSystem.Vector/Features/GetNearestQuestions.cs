using DatabaseReportingSystem.Vector.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pgvector.EntityFrameworkCore;

namespace DatabaseReportingSystem.Vector.Features;

public static class GetNearestQuestions
{
    public static IServiceCollection AddGetNearestQuestionsFeature(this IServiceCollection services)
    {
        return services.AddScoped<Feature>();
    }

    public sealed class Feature(VectorDbContext dbContext, CreateEmbedding.Feature createEmbeddingFeature)
    {
        private readonly CreateEmbedding.Feature _createEmbeddingFeature = createEmbeddingFeature;
        private readonly VectorDbContext _dbContext = dbContext;

        public async Task<List<NearestQuestionDto>> GetNearestQuestionsAsync(Request request)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(request.Question, nameof(request.Question));

            var questionVector = await _createEmbeddingFeature.GetEmbeddingVectorAsync(request.Question);

            var pgVectorEmbedding = new Pgvector.Vector(questionVector);

            var nearestQuestions = await _dbContext.Embeddings
                .Include(e => e.Schema)
                .OrderBy(e => e.Embedding.L2Distance(pgVectorEmbedding))
                .Take(5)
                .Select(e => new NearestQuestionDto(e.Question, e.Schema.Schema, e.Query))
                .ToListAsync();

            return nearestQuestions;
        }
    }

    public sealed record Request(string Question);

    public sealed record NearestQuestionDto(string Question, string Schema, string Query);
}
