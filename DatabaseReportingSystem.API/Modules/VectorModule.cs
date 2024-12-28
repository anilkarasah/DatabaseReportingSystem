using DatabaseReportingSystem.Vector.Features;
using Microsoft.AspNetCore.Mvc;

namespace DatabaseReportingSystem.Modules;

public static class VectorModule
{
    public static IEndpointRouteBuilder MapVectorModule(this IEndpointRouteBuilder builder)
    {
        RouteGroupBuilder group = builder.MapGroup("vectors");

        group.MapPost("random-questions", GetRandomQuestionsAsync);

        group.MapPost("nearest-questions", GetNearestQuestionsAsync);

        group.MapPost("embeddings", GetEmbeddingVectorAsync);

        return builder;
    }

    private static async Task<IResult> GetNearestQuestionsAsync(
        [FromServices] GetNearestQuestions.Feature feature,
        [FromBody] GetNearestQuestionsRequest request)
    {
        try
        {
            var response = await feature.GetNearestQuestionsAsync(request);

            return response.Count == 0
                ? Results.NoContent()
                : Results.Ok(response);
        }
        catch (ArgumentException e)
        {
            return Results.BadRequest(e.Message);
        }
    }

    private static async Task<IResult> GetRandomQuestionsAsync(
        [FromServices] GetRandomQuestions.Feature feature,
        [FromBody] GetRandomQuestions.Request request)
    {
        var response = await feature.GetRandomQuestionsAsync(request);

        return response.Count == 0
            ? Results.NoContent()
            : Results.Ok(response);
    }

    private static async Task<IResult> GetEmbeddingVectorAsync(
        [FromServices] CreateEmbedding.Feature feature,
        [FromBody] string question)
    {
        var response = await feature.GetEmbeddingVectorAsync(question);

        return Results.Ok(response.ToArray());
    }
}
