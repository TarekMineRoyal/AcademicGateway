using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Common.Behaviors;

/// <summary>
/// A cross-cutting MediatR pipeline middleware behavior that intercepts incoming requests before they reach their handlers.
/// Automatically discovers and executes all registered <see cref="IValidator{TRequest}"/> instances concurrently,
/// short-circuiting the execution pipeline by throwing a <see cref="ValidationException"/> if any rules are violated.
/// </summary>
/// <typeparam name="TRequest">The type of the incoming command or query request undergoing evaluation.</typeparam>
/// <typeparam name="TResponse">The type of the response payload expected upon successful execution.</typeparam>
public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <summary>
    /// Intercepts the execution pipeline to run pre-handler structural validation rules.
    /// </summary>
    /// <param name="request">The incoming command or query request message payload.</param>
    /// <param name="next">The asynchronous delegate representing the next behavior layer or target handler in the request pipeline.</param>
    /// <param name="cancellationToken">Propagates notification that network operations should be canceled.</param>
    /// <returns>The evaluated <typeparamref name="TResponse"/> payload returned from the inner request execution layers.</returns>
    /// <exception cref="ValidationException">Thrown if one or more structural or business validation rules fail execution.</exception>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // 1. Optimize execution path: Skip building context arrays if no validators exist for this type
        if (validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);

            // 2. Execute all matching validators concurrently to maximize throughput
            var validationResults = await Task.WhenAll(
                validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            // 3. Flatten the errors sequence into a single collection array
            var failures = validationResults
                .Where(r => r.Errors.Any())
                .SelectMany(r => r.Errors)
                .ToList();

            // 4. Fail-Fast Boundary: Intercept and crash the request loop BEFORE hitting down-tier handlers or database pools
            if (failures.Any())
            {
                throw new ValidationException(failures);
            }
        }

        // 5. Hand execution off to the next pipeline layer or final application handler
        return await next();
    }
}