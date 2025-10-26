namespace Diiwo.Core.Exceptions;

/// <summary>
/// Exception thrown when a user is not authorized to perform an action
/// </summary>
public class UnauthorizedException : BusinessException
{
    /// <summary>
    /// Initializes a new instance with a default message
    /// </summary>
    public UnauthorizedException() : base("Unauthorized access")
    {
    }

    /// <summary>
    /// Initializes a new instance with a custom message
    /// </summary>
    public UnauthorizedException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance with a message and inner exception
    /// </summary>
    public UnauthorizedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
