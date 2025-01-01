using DatabaseReportingSystem.Features.Chats;

namespace DatabaseReportingSystem.Modules;

public static class ChatModule
{
    public static IEndpointRouteBuilder MapChatModule(this IEndpointRouteBuilder builder)
    {
        RouteGroupBuilder group = builder.MapGroup("chats");

        group.MapGet("/", GetChatsFeature.GetChatsAsync);

        group.MapGet("{chatId:guid}", GetChatByIdFeature.GetChatAsync);

        group.MapPost("/", CreateChatFeature.CreateChatAsync);

        group.MapPost("{chatId:guid}/messages", SendMessageFeature.SendMessageAsync);

        group.MapPost("{chatId:guid}/messages/ask", AskFeature.AskAsync);

        group.MapPost("{chatId:guid}/messages/ask-autogen", AskFeature.AskUsingAutoGenAsync);

        return builder;
    }
}
