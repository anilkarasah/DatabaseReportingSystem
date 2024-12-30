using System.Text.Json.Serialization;
using DatabaseReportingSystem;
using DatabaseReportingSystem.Context;
using DatabaseReportingSystem.Vector.Context;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<SystemDbContext>();
builder.Services.AddDbContext<VectorDbContext>();

builder.Services.AddDatabaseReportingSystem();

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(o =>
{
    o.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapDatabaseReportingSystem();

app.Run();
