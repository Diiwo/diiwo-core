namespace Diiwo.Core.Responses;

/// <summary>
/// Standard API response wrapper for consistent response formatting
/// </summary>
/// <typeparam name="T">The type of data being returned</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Indicates whether the operation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Optional message describing the result
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// The actual data payload
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// List of error messages if the operation failed
    /// </summary>
    public List<string>? Errors { get; set; }

    /// <summary>
    /// Timestamp when the response was generated
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a successful response with data
    /// </summary>
    public static ApiResponse<T> SuccessResponse(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    /// <summary>
    /// Creates an error response with message and optional error details
    /// </summary>
    public static ApiResponse<T> ErrorResponse(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors
        };
    }

    /// <summary>
    /// Creates an error response from an exception
    /// </summary>
    public static ApiResponse<T> ErrorResponse(Exception exception)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = exception.Message,
            Errors = new List<string> { exception.ToString() }
        };
    }
}
