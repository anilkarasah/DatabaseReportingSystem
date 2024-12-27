namespace DatabaseReportingSystem.Vector.Models;

public sealed class SpiderSchema
{
    public string DatabaseId { get; set; }

    public string Schema { get; set; }

    public List<SpiderEmbedding> Embeddings { get; set; } = [];
}
