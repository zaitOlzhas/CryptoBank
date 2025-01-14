using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using CryptoBank_WebApi.Errors.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CryptoBank_WebApi.Errors.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder MapProblemDetailsComplete(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(builder =>
        {
            builder.Run(async context =>
            {
                var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>()!;
                var exception = exceptionHandlerPathFeature.Error;

                switch (exception)
                {
                    case ValidationErrorsException validationErrorsException:
                    {
                        await context.WriteProblemDetailsAsync(
                            "Validation failed", 
                            "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/400", 
                            validationErrorsException.Message, 
                            StatusCodes.Status400BadRequest, 
                            validationErrorsException.Errors.Select(x => new ErrorDataWithCode(x.Field, x.Message, x.Code)));
                        break;
                    }
                    case LogicConflictException logicConflictException:
                        await context.WriteProblemDetailsAsync(
                            "Logic conflict", 
                            "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/422", 
                            logicConflictException.Message, 
                            StatusCodes.Status422UnprocessableEntity, 
                            logicConflictException.Code);
                        break;
                    case OperationCanceledException:
                        await context.WriteProblemDetailsAsync( 
                            "Timeout", 
                            "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/504", 
                            "Request timed out", 
                            StatusCodes.Status504GatewayTimeout);
                        break;
                    default:
                        await context.WriteProblemDetailsAsync( 
                            "Internal server error", 
                            "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/500", 
                            exception.Message, 
                            StatusCodes.Status500InternalServerError);
                        break;
                }
            });
        });
        return app;
    }
}

internal record ErrorDataWithCode(
    [property: JsonPropertyName("field")] string Field,
    [property: JsonPropertyName("message")]
    string Message,
    [property: JsonPropertyName("code")] string Code);