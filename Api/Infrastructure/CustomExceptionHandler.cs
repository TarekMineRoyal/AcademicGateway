using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Api.Infrastructure;

public class CustomExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
    HttpContext httpContext,
    Exception exception,
    CancellationToken cancellationToken)
    {
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "An Error Occurred",
            Detail = exception.Message // <-- This will show the exact error text
        };

        if (exception is ValidationException validationException)
        {
            problemDetails.Title = "Validation Failed";
            problemDetails.Extensions["errors"] = validationException.Errors
                .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                .ToDictionary(g => g.Key, g => g.ToArray());
        }
        else
        {
            // For non-validation exceptions, include the full stack trace or inner exceptions
            problemDetails.Extensions["exceptionType"] = exception.GetType().Name;
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            if (exception.InnerException != null)
            {
                problemDetails.Extensions["innerException"] = exception.InnerException.Message;
            }
        }

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true; // Force it to handle everything cleanly for now
    }
}