using AcademicGateway.Domain.Common.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Infrastructure;

/// <summary>
/// Implements a global centralized exception handling middleware layer using ASP.NET Core's native <see cref="IExceptionHandler"/>.
/// Automatically intercepts unhandled application failures, translates exceptions into standardized RFC-compliant <see cref="ProblemDetails"/> payloads,
/// and ensures machine-readable error context codes are streamed back cleanly to consumer clients.
/// </summary>
public class CustomExceptionHandler : IExceptionHandler
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
        // Dynamically determine the status code and title based on the incoming exception inheritance type matrix
        var (statusCode, title) = exception switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, "Validation Failed"),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource Not Found"),
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
            Detail = exception.Message
        };

        // Process payload structural conditions depending on data context requirements
        if (exception is ValidationException validationException)
        {
            problemDetails.Extensions["errors"] = validationException.Errors
                .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                .ToDictionary(g => g.Key, g => g.ToArray());
        }
        else if (exception is DomainException domainException)
        {
            // Inject the machine-readable programmatic string token error key for frontend client state machines
            problemDetails.Extensions["code"] = domainException.ErrorCode;
        }
        else
        {
            // Capture general infrastructure diagnostics and debugging telemetry stack data for non-validation errors
            problemDetails.Extensions["exceptionType"] = exception.GetType().Name;
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            if (exception.InnerException != null)
            {
                problemDetails.Extensions["innerException"] = exception.InnerException.Message;
            }
        }

        // Stream the resulting ProblemDetails entity straight onto the response thread pipeline JSON stream
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}