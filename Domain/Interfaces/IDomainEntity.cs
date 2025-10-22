namespace Diiwo.Core.Domain.Interfaces;

/// <summary>
/// Complete domain entity interface that combines all entity capabilities
/// Used for consistency across different architectures (App, AspNet, etc.)
/// Inherits audit tracking from IUserTracked (which includes IAuditable and ISoftDeletable)
/// </summary>
public interface IDomainEntity : IUserTracked
{
    /// <summary>
    /// Unique identifier for the entity
    /// </summary>
    Guid Id { get; set; }

    /// <summary>
    /// Soft delete the entity
    /// </summary>
    void SoftDelete();

    /// <summary>
    /// Restore a soft-deleted entity
    /// </summary>
    void Restore();
}
