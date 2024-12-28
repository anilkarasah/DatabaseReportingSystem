using DatabaseReportingSystem.Vector.Features;
using Microsoft.AspNetCore.Mvc;

namespace DatabaseReportingSystem.Modules;

public static class VectorModule
{
    public static IEndpointRouteBuilder MapVectorModule(this IEndpointRouteBuilder builder)
    {
        RouteGroupBuilder group = builder.MapGroup("vectors");

        group.MapPost("nearest-questions",
            async (GetNearestQuestions.Feature feature, GetNearestQuestions.Request request) =>
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
            });

        group.MapPost("embeddings",
            async (CreateEmbedding.Feature feature, [FromBody] string question) =>
            {
                var response = await feature.GetEmbeddingVectorAsync(question);

                return Results.Ok(response.ToArray());
            });

        return builder;
    }
}
