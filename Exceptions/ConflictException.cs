namespace Diiwo.Core.Exceptions;

/// <summary>
/// Exception thrown when there is a conflict with existing data
/// </summary>
public class ConflictException : BusinessException
{
    /// <summary>
    /// Initializes a new instance with a default message
    /// </summary>
    public ConflictException() : base("A conflict occurred with existing data")
    {
    }

    /// <summary>
    /// Initializes a new instance with a custom message
    /// </summary>
    public ConflictException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance for a specific entity conflict
    /// </summary>
    public ConflictException(string entityName, string conflictField, object conflictValue)
        : base($"{entityName} with {conflictField} '{conflictValue}' already exists")
    {
    }

    /// <summary>
    /// Initializes a new instance with a message and inner exception
    /// </summary>
    public ConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
