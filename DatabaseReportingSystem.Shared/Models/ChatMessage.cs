namespace DatabaseReportingSystem.Shared.Models;

public sealed class ChatMessage
{
    public Guid MessageId { get; set; }

    public Guid ChatId { get; set; }

    public int Index { get; set; } = 0;

    public string Content { get; set; } = string.Empty;

    public DateTimeOffset SentAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public Chat Chat { get; set; } = null!;

    public List<ModelResponse> ModelResponses { get; set; } = [];
}
