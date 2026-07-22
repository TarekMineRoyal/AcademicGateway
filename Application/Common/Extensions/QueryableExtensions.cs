using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademicGateway.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace AcademicGateway.Application.Common.Extensions;

/// <summary>
/// Provides extension methods for EF Core <see cref="IQueryable{T}"/> queries.
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Asynchronously creates a <see cref="PaginatedResult{T}"/> from an <see cref="IQueryable{T}"/> by executing count and paged data queries.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The source <see cref="IQueryable{T}"/> query.</param>
    /// <param name="pageNumber">The 1-based page index to retrieve.</param>
    /// <param name="pageSize">The maximum number of items to retrieve per page.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="PaginatedResult{T}"/>.</returns>
    public static async Task<PaginatedResult<T>> ToPaginatedListAsync<T>(
        this IQueryable<T> source,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var count = await source.CountAsync(cancellationToken);
        var items = await source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<T>(items, count, pageNumber, pageSize);
    }
}