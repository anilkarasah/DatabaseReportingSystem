using DatabaseReportingSystem.Context;
using DatabaseReportingSystem.Modules;
using DatabaseReportingSystem.Vector.Context;
using DatabaseReportingSystem.Vector.Features;
using DatabaseReportingSystem.Vector.Shared;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<SystemDbContext>();
builder.Services.AddDbContext<VectorDbContext>();

builder.Services
    .AddVector();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app
    .MapVectorModule()
    .MapPromptModule();

app.Run();
