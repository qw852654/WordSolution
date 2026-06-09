using Microsoft.AspNetCore.Mvc;
using WordSolution.CmsV2.Application.Common;
using WordSolution.CmsV2.Domain.Exceptions;

namespace WordSolution.CmsV2.Api;

public static class CmsV2ExceptionHandlingExtensions
{
    public static IApplicationBuilder UseCmsV2ExceptionHandling(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            try
            {
                await next(context);
            }
            catch (Exception exception)
            {
                await WriteProblemAsync(context, exception);
            }
        });
    }

    private static async Task WriteProblemAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            DomainException => (StatusCodes.Status400BadRequest, "Domain validation failed."),
            CmsV2ApplicationException => (StatusCodes.Status400BadRequest, "Application validation failed."),
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request."),
            _ => (StatusCodes.Status500InternalServerError, "Unexpected server error.")
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message
        });
    }
}
