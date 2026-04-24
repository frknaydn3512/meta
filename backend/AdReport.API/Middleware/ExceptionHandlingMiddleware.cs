using AdReport.Application.Common;
using System.Net;
using System.Text.Json;

namespace AdReport.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            ArgumentNullException => new { statusCode = HttpStatusCode.BadRequest, apiResponse = ApiResponse.Error("Invalid input provided") },
            ArgumentException => new { statusCode = HttpStatusCode.BadRequest, apiResponse = ApiResponse.Error(exception.Message) },
            UnauthorizedAccessException => new { statusCode = HttpStatusCode.Unauthorized, apiResponse = ApiResponse.Error("Unauthorized access") },
            KeyNotFoundException => new { statusCode = HttpStatusCode.NotFound, apiResponse = ApiResponse.Error("Resource not found") },
            _ => new { statusCode = HttpStatusCode.InternalServerError, apiResponse = ApiResponse.Error("An error occurred while processing your request") }
        };

        context.Response.StatusCode = (int)response.statusCode;

        var jsonResponse = JsonSerializer.Serialize(response.apiResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}