using System.ClientModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAI.Embeddings;

namespace DatabaseReportingSystem.Vector.Features;

public static class CreateEmbedding
{
    public static IServiceCollection AddCreateEmbeddingFeature(this IServiceCollection services)
    {
        return services.AddScoped<Feature>();
    }

    public sealed class Feature(IConfiguration configuration)
    {
        private readonly ApiKeyCredential _credentials = new(configuration.GetConnectionString("GptApiKey")!);

        public async Task<ReadOnlyMemory<float>> GetEmbeddingVectorAsync(string question)
        {
            EmbeddingClient embeddingClient = new("text-embedding-3-small", _credentials);

            ClientResult<OpenAIEmbedding>? embedding = await embeddingClient.GenerateEmbeddingAsync(question);

            if (embedding is null) throw new InvalidOperationException("Failed to generate an embedding.");

            return embedding.Value.ToFloats();
        }
    }
}
