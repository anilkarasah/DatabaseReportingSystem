using EntityFramework.Exceptions.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatabaseReportingSystem;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

        int statusCode = StatusCodes.Status500InternalServerError;
        string title = "Unexpected error";
        string detail = "An unexpected error occurred while processing the request.";

        switch (exception)
        {
            case UniqueConstraintException:
                statusCode = StatusCodes.Status400BadRequest;
                title = "Unique constraint violation";
                detail = "A unique constraint violation occurred while processing the request.";
                break;

            case DbUpdateException:
                statusCode = StatusCodes.Status400BadRequest;
                title = "Database update error";
                detail = "An error occurred while updating the database.";
                break;
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
        };

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
