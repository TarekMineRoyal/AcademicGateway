using System;
using System.Collections.Generic;

namespace AcademicGateway.Application.Common.Models;

/// <summary>
/// Generic container encapsulating paginated query results.
/// </summary>
/// <typeparam name="T">The type of the paginated items.</typeparam>
public class PaginatedResult<T>
{
    /// <summary>
    /// Gets the collection of items for the current page.
    /// </summary>
    public IReadOnlyList<T> Items { get; }

    /// <summary>
    /// Gets the current 1-based page number.
    /// </summary>
    public int PageNumber { get; }

    /// <summary>
    /// Gets the total number of items available across all pages.
    /// </summary>
    public int TotalCount { get; }

    /// <summary>
    /// Gets the total number of pages calculated from <see cref="TotalCount"/> and <see cref="PageSize"/>.
    /// </summary>
    public int TotalPages { get; }

    /// <summary>
    /// Gets the configured size limit per page.
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// Gets a value indicating whether a previous page exists.
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Gets a value indicating whether a next page exists.
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Initializes a new instance of the <see cref="PaginatedResult{T}"/> class.
    /// </summary>
    /// <param name="items">The current page of items.</param>
    /// <param name="totalCount">The total count of matching records in the database.</param>
    /// <param name="pageNumber">The current page number (1-based).</param>
    /// <param name="pageSize">The page size limit.</param>
    public PaginatedResult(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalPages = pageSize > 0 ? (int)Math.Ceiling(totalCount / (double)pageSize) : 0;
    }
}