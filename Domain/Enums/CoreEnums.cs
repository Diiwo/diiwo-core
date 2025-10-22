using System.ComponentModel;

namespace Diiwo.Core.Domain.Enums;

/// <summary>
/// Universal entity states for all domain entities
/// </summary>
public enum EntityState
{
    /// <summary>
    /// Created but not yet active
    /// </summary>
    [Description("Created but not yet active")]
    Created = 0,

    /// <summary>
    /// Temporarily inactive
    /// </summary>
    [Description("Temporarily inactive")]
    Inactive = 1,

    /// <summary>
    /// Active and available
    /// </summary>
    [Description("Active and available")]
    Active = 2,

    /// <summary>
    /// Effective and operational
    /// </summary>
    [Description("Effective and operational")]
    Effective = 3,

    /// <summary>
    /// Soft deleted/terminated
    /// </summary>
    [Description("Soft deleted/terminated")]
    Terminated = 4
}
