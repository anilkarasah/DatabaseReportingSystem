namespace DatabaseReportingSystem.Vector.Models;

public sealed class SpiderEmbedding
{
    public int Id { get; set; }

    public required string DatabaseId { get; set; }

    public string Question { get; set; } = string.Empty;

    public string Query { get; set; } = string.Empty;

    public required Pgvector.Vector Embedding { get; set; }

    public SpiderSchema Schema { get; set; } = null!;
}
