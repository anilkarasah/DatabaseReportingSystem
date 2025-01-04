using DatabaseReportingSystem.Shared.Settings;
using DatabaseReportingSystem.Vector.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DatabaseReportingSystem.Vector.Context;

public class VectorDbContext(IOptions<ConnectionStrings> connectionStrings) : DbContext
{
    private readonly ConnectionStrings _connectionStrings = connectionStrings.Value;

    public DbSet<SpiderSchema> Schemas { get; set; }

    public DbSet<SpiderEmbedding> Embeddings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_connectionStrings.Vector, o => o.UseVector());
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("vector");

        modelBuilder.Entity<SpiderSchema>()
            .HasKey(s => s.DatabaseId);

        modelBuilder.Entity<SpiderSchema>()
            .HasMany(s => s.Embeddings)
            .WithOne(e => e.Schema)
            .HasForeignKey(e => e.DatabaseId);

        modelBuilder.Entity<SpiderSchema>()
            .Property(s => s.DatabaseId)
            .HasMaxLength(128);

        modelBuilder.Entity<SpiderEmbedding>()
            .HasKey(e => e.Id);

        modelBuilder.Entity<SpiderEmbedding>()
            .Property(embedding => embedding.Embedding)
            .HasColumnType("vector(1536)");

        modelBuilder.Entity<SpiderEmbedding>()
            .HasIndex(e => e.Embedding)
            .HasMethod("ivfflat")
            .HasOperators("vector_l2_ops")
            .HasStorageParameter("lists", 7);

        modelBuilder.Entity<SpiderEmbedding>()
            .Property(s => s.DatabaseId)
            .HasMaxLength(128);
    }
}
