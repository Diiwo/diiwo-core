using Diiwo.Core.Domain.Enums;

namespace Diiwo.Core.Domain.Interfaces;

/// <summary>
/// Interface for entities that can be soft deleted
/// </summary>
public interface ISoftDeletable
{
    /// <summary>
    /// Current state of the entity
    /// </summary>
    EntityState State { get; set; }
    
    /// <summary>
    /// Indicates whether the entity has been soft deleted
    /// </summary>
    bool IsTerminated => State == EntityState.Terminated;
}

/// <summary>
/// Interface for auditable entities
/// </summary>
public interface IAuditable : ISoftDeletable
{
    /// <summary>
    /// Date and time when the entity was created
    /// </summary>
    DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Date and time when the entity was last updated
    /// </summary>
    DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Interface for user-tracked entities
/// </summary>
public interface IUserTracked : IAuditable
{
    /// <summary>
    /// ID of the user who created the entity
    /// </summary>
    Guid? CreatedBy { get; set; }
    
    /// <summary>
    /// ID of the user who last updated the entity
    /// </summary>
    Guid? UpdatedBy { get; set; }
}

/// <summary>
/// Interface for entities owned by users (multi-tenant)
/// </summary>
public interface IUserOwned
{
    /// <summary>
    /// ID of the user who owns this entity
    /// </summary>
    Guid? UserId { get; set; }
}

/// <summary>
/// Interface for current user service
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// ID of the current authenticated user
    /// </summary>
    Guid? UserId { get; }
    
    /// <summary>
    /// Name of the current authenticated user
    /// </summary>
    string? UserName { get; }

    /// <summary>
    /// Email of the current authenticated user
    /// </summary>
    string? UserEmail { get; }

    /// <summary>
    /// Indicates whether a user is currently authenticated
    /// </summary>
    bool IsAuthenticated { get; }
    
    /// <summary>
    /// Checks if the current user is in the specified role
    /// </summary>
    /// <param name="role">Role name to check</param>
    /// <returns>True if user is in role, false otherwise</returns>
    Task<bool> IsInRoleAsync(string role);
}