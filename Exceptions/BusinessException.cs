namespace Diiwo.Core.Exceptions;

/// <summary>
/// Base exception for business logic violations
/// </summary>
public class BusinessException : Exception
{
    /// <summary>
    /// Initializes a new instance of BusinessException
    /// </summary>
    public BusinessException()
    {
    }

    /// <summary>
    /// Initializes a new instance of BusinessException with a message
    /// </summary>
    public BusinessException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of BusinessException with a message and inner exception
    /// </summary>
    public BusinessException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
