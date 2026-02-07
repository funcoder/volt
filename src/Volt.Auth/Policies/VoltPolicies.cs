using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Volt.Auth.Policies;

/// <summary>
/// Provides common authorization policy names and registration for the Volt framework.
/// Use these constants with <c>[Authorize(Policy = ...)]</c> attributes.
/// </summary>
public static class VoltPolicies
{
    /// <summary>
    /// Policy name requiring the user to be authenticated.
    /// </summary>
    public const string IsAuthenticated = nameof(IsAuthenticated);

    /// <summary>
    /// Policy name requiring the user to have the "Admin" role.
    /// </summary>
    public const string IsAdmin = nameof(IsAdmin);

    /// <summary>
    /// Policy name requiring the current user to own the requested resource.
    /// The resource must supply an "OwnerId" claim or route value matching the user's ID.
    /// </summary>
    public const string IsOwner = nameof(IsOwner);

    /// <summary>
    /// Registers Volt's built-in authorization policies on the provided options.
    /// </summary>
    /// <param name="options">The authorization options to configure.</param>
    internal static void Register(AuthorizationOptions options)
    {
        options.AddPolicy(IsAuthenticated, policy =>
            policy.RequireAuthenticatedUser());

        options.AddPolicy(IsAdmin, policy =>
            policy.RequireRole("Admin"));

        options.AddPolicy(IsOwner, policy =>
            policy.RequireAssertion(context =>
            {
                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return false;
                }

                if (context.Resource is HttpContext httpContext)
                {
                    var ownerId = httpContext.Request.RouteValues
                        .GetValueOrDefault("userId")?.ToString();

                    return string.Equals(userId, ownerId, StringComparison.Ordinal);
                }

                return false;
            }));
    }
}
