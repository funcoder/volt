using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Volt.Auth.Policies;
using Volt.Data;

namespace Volt.Auth.Extensions;

/// <summary>
/// Extension methods for registering and configuring Volt authentication services.
/// </summary>
public static class VoltAuthExtensions
{
    /// <summary>
    /// Adds Volt's opinionated Identity configuration to the service collection.
    /// Registers <see cref="VoltUser"/> and <see cref="VoltRole"/> with Entity Framework stores,
    /// configures password, lockout, sign-in, and cookie settings from <see cref="VoltAuthOptions"/>.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configure">Optional action to override default <see cref="VoltAuthOptions"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddVoltAuth(
        this IServiceCollection services,
        Action<VoltAuthOptions>? configure = null)
    {
        var options = new VoltAuthOptions();
        configure?.Invoke(options);

        services.AddIdentity<VoltUser, VoltRole>(identityOptions =>
            {
                ConfigurePassword(identityOptions, options);
                ConfigureLockout(identityOptions, options);
                ConfigureSignIn(identityOptions, options);
                ConfigureUser(identityOptions);
            })
            .AddEntityFrameworkStores<VoltDbContext>()
            .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(cookie =>
        {
            cookie.LoginPath = "/login";
            cookie.LogoutPath = "/logout";
            cookie.AccessDeniedPath = "/access-denied";
            cookie.ExpireTimeSpan = options.SessionTimeout;
            cookie.SlidingExpiration = true;
            cookie.Cookie.HttpOnly = true;
            cookie.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
        });

        services.AddAuthorization(VoltPolicies.Register);

        return services;
    }

    /// <summary>
    /// Adds authentication and authorization middleware to the application pipeline.
    /// Must be called after <c>UseRouting</c> and before <c>MapControllers</c> / <c>MapRazorPages</c>.
    /// </summary>
    /// <param name="app">The web application to configure.</param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication UseVoltAuth(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }

    private static void ConfigurePassword(IdentityOptions identity, VoltAuthOptions options)
    {
        identity.Password.RequiredLength = options.PasswordMinLength;
        identity.Password.RequireUppercase = options.RequireUppercase;
        identity.Password.RequireDigit = options.RequireDigit;
        identity.Password.RequireLowercase = true;
        identity.Password.RequireNonAlphanumeric = false;
    }

    private static void ConfigureLockout(IdentityOptions identity, VoltAuthOptions options)
    {
        identity.Lockout.MaxFailedAccessAttempts = options.LockoutMaxFailedAttempts;
        identity.Lockout.DefaultLockoutTimeSpan = options.LockoutDuration;
        identity.Lockout.AllowedForNewUsers = true;
    }

    private static void ConfigureSignIn(IdentityOptions identity, VoltAuthOptions options)
    {
        identity.SignIn.RequireConfirmedEmail = options.RequireConfirmedEmail;
    }

    private static void ConfigureUser(IdentityOptions identity)
    {
        identity.User.RequireUniqueEmail = true;
    }
}
