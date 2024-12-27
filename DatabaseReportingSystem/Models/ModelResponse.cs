namespace DatabaseReportingSystem.Models;

public sealed class ModelResponse
{
    public Guid MessageId { get; set; }

    public string ModelName { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTimeOffset CompletedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public ChatMessage Message { get; set; } = null!;
}
