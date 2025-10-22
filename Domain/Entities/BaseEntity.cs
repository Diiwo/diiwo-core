using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Diiwo.Core.Domain.Enums;
using Diiwo.Core.Domain.Interfaces;

namespace Diiwo.Core.Domain.Entities;

/// <summary>
/// Base entity class with basic properties for all domain entities
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique identifier for the entity
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Date and time when the entity was created (UTC)
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the entity was last updated (UTC)
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Auditable entity with state management, and soft delete
/// Universal base for entities that need audit trails and soft delete
/// </summary>
public abstract class AuditableEntity : BaseEntity, IAuditable
{
    /// <summary>
    /// Current state of the entity (Active, Inactive, Terminated, etc.)
    /// </summary>
    public EntityState State { get; set; } = EntityState.Active;

    /// <summary>
    /// Soft delete the entity
    /// </summary>
    public virtual void SoftDelete()
    {
        State = EntityState.Terminated;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Restore a soft-deleted entity
    /// </summary>
    public virtual void Restore()
    {
        State = EntityState.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Check if entity is active
    /// </summary>
    [NotMapped]
    public bool IsActive => State == EntityState.Active;

    /// <summary>
    /// Check if entity is inactive
    /// </summary>
    [NotMapped]
    public bool IsInactive => State == EntityState.Inactive;

    /// <summary>
    /// Indicates whether the entity has been soft deleted (terminated)
    /// </summary>
    [NotMapped]
    public bool IsTerminated => State == EntityState.Terminated;
}

/// <summary>
/// User-tracked entity with created/updated by information
/// Universal base for entities that need to track who created/modified them
/// </summary>
public abstract class UserTrackedEntity : AuditableEntity, IUserTracked
{
    /// <summary>
    /// ID of the user who created this entity
    /// </summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// ID of the user who last updated this entity
    /// </summary>
    public Guid? UpdatedBy { get; set; }
}

/// <summary>
/// Domain entity base class for business entities
/// Universal base for all business domain entities
/// Implements IDomainEntity for consistency across architectures
/// </summary>
public abstract class DomainEntity : UserTrackedEntity, IDomainEntity
{
    // Additional domain-specific functionality can be added here
    // This is the recommended base for most business entities
}

/// <summary>
/// User-owned entity for multi-tenant scenarios
/// Universal base for entities that belong to specific users
/// </summary>
public abstract class UserOwnedEntity : UserTrackedEntity, IUserOwned
{
    /// <summary>
    /// ID of the user who owns this entity (null for global/shared entities)
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Check if entity is owned by a specific user
    /// </summary>
    /// <param name="userId">User ID to check</param>
    /// <returns>True if owned by the user or is global (UserId is null)</returns>
    public bool IsOwnedBy(Guid? userId)
    {
        return UserId == userId || UserId == null;
    }

    /// <summary>
    /// Check if entity is global (not owned by any specific user)
    /// </summary>
    public bool IsGlobal => UserId == null;
}