using Microsoft.AspNetCore.Identity;

namespace Volt.Auth;

/// <summary>
/// Default Volt user extending ASP.NET Core Identity with timestamps and conventions.
/// Follows the same timestamp pattern as <see cref="Volt.Core.Model{T}"/> for consistency
/// across Identity and domain models.
/// </summary>
public class VoltUser : IdentityUser
{
    /// <summary>
    /// The timestamp when this user was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The timestamp when this user was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; init; }

    /// <summary>
    /// The timestamp when this user was soft-deleted, or null if active.
    /// </summary>
    public DateTime? DeletedAt { get; init; }
}
