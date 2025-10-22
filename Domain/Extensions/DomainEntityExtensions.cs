using Diiwo.Core.Domain.Enums;
using Diiwo.Core.Domain.Interfaces;

namespace Diiwo.Core.Domain.Extensions;

/// <summary>
/// Extension methods for IDomainEntity to provide common domain entity behaviors
/// Useful for entities that can't inherit from DomainEntity (like AspNet Identity entities)
/// </summary>
public static class DomainEntityExtensions
{
    /// <summary>
    /// Soft delete the entity by setting its state to Terminated
    /// </summary>
    public static void SoftDelete(this IDomainEntity entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        entity.State = EntityState.Terminated;
        entity.UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Restore a soft-deleted entity by setting its state back to Active
    /// </summary>
    public static void Restore(this IDomainEntity entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        entity.State = EntityState.Active;
        entity.UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Check if entity is active
    /// </summary>
    public static bool IsActive(this IDomainEntity entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        return entity.State == EntityState.Active;
    }

    /// <summary>
    /// Check if entity is inactive
    /// </summary>
    public static bool IsInactive(this IDomainEntity entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        return entity.State == EntityState.Inactive;
    }

    /// <summary>
    /// Check if entity is terminated (soft deleted)
    /// </summary>
    public static bool IsTerminated(this IDomainEntity entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        return entity.State == EntityState.Terminated;
    }

    /// <summary>
    /// Set entity state to Inactive
    /// </summary>
    public static void Deactivate(this IDomainEntity entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        entity.State = EntityState.Inactive;
        entity.UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Set entity state to Active
    /// </summary>
    public static void Activate(this IDomainEntity entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        entity.State = EntityState.Active;
        entity.UpdatedAt = DateTime.UtcNow;
    }
}
