using Diiwo.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Diiwo.Core.Interceptors;

/// <summary>
/// Entity Framework interceptor for automatic audit tracking
/// Universal interceptor that auto-populates audit fields and handles soft deletes
/// </summary>
public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Initializes a new instance of the AuditInterceptor
    /// </summary>
    /// <param name="currentUserService">Service for getting current user information</param>
    public AuditInterceptor(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Intercepts synchronous save changes to update audit fields
    /// </summary>
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateAuditFields(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <summary>
    /// Intercepts asynchronous save changes to update audit fields
    /// </summary>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, 
        InterceptionResult<int> result, 
        CancellationToken cancellationToken = default)
    {
        UpdateAuditFields(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateAuditFields(DbContext? context)
    {
        if (context == null) return;

        var currentUserId = _currentUserService.UserId;
        var now = DateTime.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            // Handle IAuditable entities (BaseEntity, AuditableEntity, etc.)
            if (entry.Entity is IAuditable auditableEntity)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        auditableEntity.CreatedAt = now;
                        auditableEntity.UpdatedAt = now;
                        break;

                    case EntityState.Modified:
                        auditableEntity.UpdatedAt = now;
                        // Prevent modification of CreatedAt
                        entry.Property(nameof(IAuditable.CreatedAt)).IsModified = false;
                        break;
                }
            }

            // Handle IUserTracked entities (UserTrackedEntity, DomainEntity, etc.)
            if (entry.Entity is IUserTracked userTrackedEntity && currentUserId.HasValue)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        userTrackedEntity.CreatedBy = currentUserId;
                        userTrackedEntity.UpdatedBy = currentUserId;
                        break;

                    case EntityState.Modified:
                        userTrackedEntity.UpdatedBy = currentUserId;
                        // Prevent modification of CreatedBy
                        entry.Property(nameof(IUserTracked.CreatedBy)).IsModified = false;
                        break;
                }
            }

            // Handle soft deletes for ISoftDeletable entities
            if (entry.State == EntityState.Deleted && entry.Entity is ISoftDeletable softDeletableEntity)
            {
                // Instead of hard delete, perform soft delete
                entry.State = EntityState.Modified;
                softDeletableEntity.State = Domain.Enums.EntityState.Terminated;

                // Update audit fields for soft delete
                if (entry.Entity is IAuditable softDeleteAuditable)
                {
                    softDeleteAuditable.UpdatedAt = now;
                }

                if (entry.Entity is IUserTracked softDeleteUserTracked && currentUserId.HasValue)
                {
                    softDeleteUserTracked.UpdatedBy = currentUserId;
                }
            }
        }
    }
}