using System.Linq.Expressions;

namespace Volt.Data.Extensions;

/// <summary>
/// Extension methods for <see cref="IQueryable{T}"/> that provide
/// Rails-like query helpers such as pagination and conditional filtering.
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Returns a page of results from the queryable source.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The queryable source.</param>
    /// <param name="page">The 1-based page number. Values less than 1 are clamped to 1.</param>
    /// <param name="perPage">The number of items per page. Defaults to 25.</param>
    /// <returns>A queryable representing the requested page.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="perPage"/> is less than 1.
    /// </exception>
    public static IQueryable<T> Page<T>(this IQueryable<T> source, int page, int perPage = 25)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (perPage < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(perPage), perPage, "Per-page count must be at least 1.");
        }

        var safePage = Math.Max(1, page);
        var skip = (safePage - 1) * perPage;

        return source.Skip(skip).Take(perPage);
    }

    /// <summary>
    /// Conditionally applies a where-clause predicate to the queryable source.
    /// When <paramref name="condition"/> is <c>false</c>, the source is returned unchanged.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The queryable source.</param>
    /// <param name="condition">Whether to apply the predicate.</param>
    /// <param name="predicate">The filter expression to apply when <paramref name="condition"/> is <c>true</c>.</param>
    /// <returns>The filtered or unfiltered queryable.</returns>
    public static IQueryable<T> WhereIf<T>(
        this IQueryable<T> source,
        bool condition,
        Expression<Func<T, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        return condition ? source.Where(predicate) : source;
    }
}
