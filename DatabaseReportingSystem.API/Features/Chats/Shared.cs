using DatabaseReportingSystem.Shared;
using DatabaseReportingSystem.Shared.Models;

namespace DatabaseReportingSystem.Features.Chats;

public static class Shared
{
    public sealed record ChatResponse(
        Guid Id,
        Guid UserId,
        string Name,
        List<ChatMessageResponse> Messages,
        DatabaseManagementSystem DatabaseManagementSystem,
        DateTimeOffset CreatedAtUtc)
    {
        public static ChatResponse FromChat(Chat chat)
            => new(
                chat.Id,
                chat.UserId,
                chat.Messages.FirstOrDefault()?.Content ?? string.Empty,
                chat.Messages.ConvertAll(ChatMessageResponse.FromChatMessage),
                chat.DatabaseManagementSystem,
                chat.CreatedAtUtc
            );
    }

    public sealed record ChatMessageResponse(
        Guid Id,
        Guid ChatId,
        string Content,
        int Index,
        List<ModelResponseDto> ModelResponses,
        DateTimeOffset SentAtUtc)
    {
        public static ChatMessageResponse FromChatMessage(ChatMessage message)
            => new(
                message.MessageId,
                message.ChatId,
                message.Content,
                message.Index,
                message.ModelResponses.ConvertAll(ModelResponseDto.FromModelResponse),
                message.SentAtUtc
            );
    }

    public sealed record ModelResponseDto(
        Guid MessageId,
        LargeLanguageModel LlmType,
        string Content,
        DateTimeOffset CompletedAtUtc)
    {
        public static ModelResponseDto FromModelResponse(ModelResponse modelResponse)
            => new(
                modelResponse.MessageId,
                modelResponse.ModelName.AsLargeLanguageModel(),
                modelResponse.Content,
                modelResponse.CompletedAtUtc
            );
    }
}
