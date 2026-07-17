using AcademicGateway.Domain.Common.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Infrastructure;

/// <summary>
/// Implements a global centralized exception handling middleware layer using ASP.NET Core's native <see cref="IExceptionHandler"/>.
/// Automatically intercepts unhandled application failures, translates exceptions into standardized RFC-compliant <see cref="ProblemDetails"/> payloads,
/// and ensures machine-readable error context codes are streamed back cleanly to consumer clients.
/// </summary>
public class CustomExceptionHandler(
    ILogger<CustomExceptionHandler> logger,
    IWebHostEnvironment environment) : IExceptionHandler
{
    /// <summary>
    /// Intercepts throwing system errors dynamically to map appropriate status codes, error details, and custom metadata extensions.
    /// </summary>
    /// <param name="httpContext">The active HTTP execution pipeline context wrapper.</param>
    /// <param name="exception">The unhandled exception caught by the ASP.NET diagnostic system middleware.</param>
    /// <param name="cancellationToken">The operational signal tracking asynchronous processing cancellations.</param>
    /// <returns>A value task boolean indicating whether the interceptor successfully handled and serialized the API failure payload.</returns>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Log the unhandled exception context securely on the server telemetry channel
        if (exception is DomainException || exception is ValidationException || exception is KeyNotFoundException)
        {
            logger.LogWarning(exception, "A managed business validation or domain rule rule constraint was violated: {Message}", exception.Message);
        }
        else
        {
            logger.LogError(exception, "An unhandled system exception occurred during request execution: {Message}", exception.Message);
        }

        // Dynamically determine the status code and title based on the incoming exception inheritance type matrix
        var (statusCode, title) = exception switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, "Validation Failed"),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource Not Found"),
            UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "Access Denied"), // Map security context and ownership boundary violations cleanly
            DomainException => (StatusCodes.Status409Conflict, "Domain Rule Violation"), // Safely route state invariant failures to HTTP 409 Conflict
            InvalidOperationException => (StatusCodes.Status400BadRequest, "Invalid Operation"),
            _ => (StatusCodes.Status500InternalServerError, "An Error Occurred")
        };

        httpContext.Response.StatusCode = statusCode;

        // Instantiate standard RFC-7807 Problem Details blueprint architecture
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception switch
            {
                // Intercept and sanitize database/ORM state context leaks 
                InvalidOperationException => "The requested operation could not be completed due to a server state conflict.",
                _ => exception.Message
            }
        };

        // Establish a predictable baseline contract format inside the Extensions dictionary by providing an empty fallback shape for errors
        problemDetails.Extensions["errors"] = new Dictionary<string, string[]>();

        // Append explicit payload structural metadata details depending on exception type contexts
        switch (exception)
        {
            case ValidationException validationException:
                // Normalizes property keys to standard camelCase to match our global API serialization policy contract
                problemDetails.Extensions["errors"] = validationException.Errors
                    .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                    .ToDictionary(
                        g => string.IsNullOrEmpty(g.Key) ? string.Empty : JsonNamingPolicy.CamelCase.ConvertName(g.Key),
                        g => g.ToArray());
                break;

            case DomainException domainException:
                // Inject the machine-readable programmatic string token error key for frontend client state machines
                problemDetails.Extensions["code"] = domainException.ErrorCode;
                break;

            case UnauthorizedAccessException:
                // Inject programmatic security category metadata without leaking structural details
                problemDetails.Extensions["code"] = "ACCESS_DENIED";
                break;

            default:
                // Protect Production: Only capture infrastructure diagnostics stack details while working in Development environments
                if (environment.IsDevelopment())
                {
                    problemDetails.Extensions["exceptionType"] = exception.GetType().Name;
                    problemDetails.Extensions["stackTrace"] = exception.StackTrace;
                    if (exception.InnerException != null)
                    {
                        problemDetails.Extensions["innerException"] = exception.InnerException.Message;
                    }
                }
                break;
        }

        // Stream the resulting ProblemDetails entity straight onto the response thread pipeline JSON stream
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}