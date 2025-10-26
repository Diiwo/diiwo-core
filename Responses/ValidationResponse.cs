namespace Diiwo.Core.Responses;

/// <summary>
/// Response for validation operations
/// </summary>
public class ValidationResponse : ApiResponse<object>
{
    /// <summary>
    /// Dictionary of field-specific validation errors
    /// Key: field name, Value: list of error messages for that field
    /// </summary>
    public Dictionary<string, List<string>>? ValidationErrors { get; set; }

    /// <summary>
    /// Creates a validation error response
    /// </summary>
    public static ValidationResponse Create(Dictionary<string, List<string>> validationErrors)
    {
        return new ValidationResponse
        {
            Success = false,
            Message = "Validation failed",
            ValidationErrors = validationErrors,
            Errors = validationErrors.Values.SelectMany(v => v).ToList()
        };
    }

    /// <summary>
    /// Creates a validation error response from a single field error
    /// </summary>
    public static ValidationResponse Create(string fieldName, string errorMessage)
    {
        return new ValidationResponse
        {
            Success = false,
            Message = "Validation failed",
            ValidationErrors = new Dictionary<string, List<string>>
            {
                { fieldName, new List<string> { errorMessage } }
            },
            Errors = new List<string> { errorMessage }
        };
    }
}
