namespace Diiwo.Core.Responses;

/// <summary>
/// Paginated API response for list operations
/// </summary>
/// <typeparam name="T">The type of items in the list</typeparam>
public class PagedResponse<T> : ApiResponse<List<T>>
{
    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of items across all pages
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;

    /// <summary>
    /// Indicates if there is a previous page
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Indicates if there is a next page
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// Creates a successful paged response
    /// </summary>
    public static PagedResponse<T> Create(List<T> data, int page, int pageSize, int totalCount, string? message = null)
    {
        return new PagedResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    /// <summary>
    /// Creates an error paged response
    /// </summary>
    public static new PagedResponse<T> ErrorResponse(string message, List<string>? errors = null)
    {
        return new PagedResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors,
            Data = new List<T>(),
            Page = 0,
            PageSize = 0,
            TotalCount = 0
        };
    }
}
