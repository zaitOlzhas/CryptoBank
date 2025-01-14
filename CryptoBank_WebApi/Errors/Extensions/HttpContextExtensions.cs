using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace CryptoBank_WebApi.Errors.Extensions;

public static class HttpContextExtensions
{
    public static async Task WriteProblemDetailsAsync(this HttpContext context, string title, string type, string detail, int statusCode, object? errors = null)
    {
        var problemDetails = new ProblemDetails
        {
            Title = title,
            Type = type,
            Detail = detail,
            Status = statusCode,
        };

        problemDetails.Extensions.Add("traceId", Activity.Current?.Id ?? context.TraceIdentifier);

        if (errors != null)
        {
            problemDetails.Extensions["errors"] = errors;
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
    }
}