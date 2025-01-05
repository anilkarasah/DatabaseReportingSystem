using System.Text.Json.Serialization;
using DatabaseReportingSystem;
using DatabaseReportingSystem.Context;
using DatabaseReportingSystem.Shared.Settings;
using DatabaseReportingSystem.Vector.Context;
using Microsoft.AspNetCore.Http.Json;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:8080")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin();
    });
});

builder.Services.AddDbContext<SystemDbContext>();
builder.Services.AddDbContext<VectorDbContext>();

builder.Services.AddDatabaseReportingSystem();

builder.Services.Configure<JsonOptions>(o => { o.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles; });

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddUserSecrets(typeof(Program).Assembly, optional: true)
    .AddEnvironmentVariables();

builder.Services.Configure<ConnectionStrings>(builder.Configuration.GetSection("ConnectionStrings"));
builder.Services.Configure<ApiKeys>(builder.Configuration.GetSection("ApiKeys"));
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("Settings"));

WebApplication app = builder.Build();

app.UseExceptionHandler();

app.UseCors();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapDatabaseReportingSystem();

app.Run();
