namespace Diiwo.Core.Exceptions;

/// <summary>
/// Exception thrown when a requested resource is not found
/// </summary>
public class NotFoundException : BusinessException
{
    /// <summary>
    /// Initializes a new instance with a default message
    /// </summary>
    public NotFoundException() : base("The requested resource was not found")
    {
    }

    /// <summary>
    /// Initializes a new instance with a custom message
    /// </summary>
    public NotFoundException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance for a specific entity and key
    /// </summary>
    public NotFoundException(string entityName, object key)
        : base($"{entityName} with key '{key}' was not found")
    {
    }

    /// <summary>
    /// Initializes a new instance with a message and inner exception
    /// </summary>
    public NotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
