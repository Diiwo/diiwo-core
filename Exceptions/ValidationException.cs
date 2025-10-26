namespace Diiwo.Core.Exceptions;

/// <summary>
/// Exception thrown when validation fails
/// </summary>
public class ValidationException : BusinessException
{
    /// <summary>
    /// Dictionary of field-specific validation errors
    /// </summary>
    public Dictionary<string, List<string>>? ValidationErrors { get; set; }

    /// <summary>
    /// Initializes a new instance with a default message
    /// </summary>
    public ValidationException() : base("One or more validation errors occurred")
    {
    }

    /// <summary>
    /// Initializes a new instance with a custom message
    /// </summary>
    public ValidationException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance with validation errors dictionary
    /// </summary>
    public ValidationException(Dictionary<string, List<string>> validationErrors)
        : base("One or more validation errors occurred")
    {
        ValidationErrors = validationErrors;
    }

    /// <summary>
    /// Initializes a new instance for a single field validation error
    /// </summary>
    public ValidationException(string fieldName, string errorMessage)
        : base(errorMessage)
    {
        ValidationErrors = new Dictionary<string, List<string>>
        {
            { fieldName, new List<string> { errorMessage } }
        };
    }

    /// <summary>
    /// Initializes a new instance with a message and inner exception
    /// </summary>
    public ValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
