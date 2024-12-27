using DatabaseReportingSystem.Context;
using DatabaseReportingSystem.Vector.Context;
using DatabaseReportingSystem.Vector.Features;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<SystemDbContext>();
builder.Services.AddDbContext<VectorDbContext>();

builder.Services.AddGetNearestQuestionsFeature();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

RouteGroupBuilder methodGroup = app.MapGroup("methods");

methodGroup.MapGet("nearest", async (GetNearestQuestions.Feature feature) =>
{
    var response = await feature.GetNearestQuestionsAsync(new GetNearestQuestions.Request(""));

    return response.Count == 0
        ? Results.NoContent()
        : Results.Ok(response);
});

app.Run();
