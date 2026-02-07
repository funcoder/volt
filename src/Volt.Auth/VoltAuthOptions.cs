namespace Volt.Auth;

/// <summary>
/// Configuration options for the Volt authentication system.
/// Provides opinionated defaults that can be overridden per-application.
/// </summary>
public class VoltAuthOptions
{
    /// <summary>
    /// Whether users must confirm their email address before signing in.
    /// Default: <c>false</c>.
    /// </summary>
    public bool RequireConfirmedEmail { get; init; }

    /// <summary>
    /// Minimum required password length.
    /// Default: <c>8</c>.
    /// </summary>
    public int PasswordMinLength { get; init; } = 8;

    /// <summary>
    /// Whether passwords must contain at least one uppercase letter.
    /// Default: <c>true</c>.
    /// </summary>
    public bool RequireUppercase { get; init; } = true;

    /// <summary>
    /// Whether passwords must contain at least one digit.
    /// Default: <c>true</c>.
    /// </summary>
    public bool RequireDigit { get; init; } = true;

    /// <summary>
    /// Maximum number of consecutive failed login attempts before account lockout.
    /// Default: <c>5</c>.
    /// </summary>
    public int LockoutMaxFailedAttempts { get; init; } = 5;

    /// <summary>
    /// Duration of account lockout after exceeding the maximum failed attempts.
    /// Default: <c>15 minutes</c>.
    /// </summary>
    public TimeSpan LockoutDuration { get; init; } = TimeSpan.FromMinutes(15);

    /// <summary>
    /// How long an authentication session remains valid before requiring re-login.
    /// Default: <c>30 days</c>.
    /// </summary>
    public TimeSpan SessionTimeout { get; init; } = TimeSpan.FromDays(30);

    /// <summary>
    /// Whether to enable two-factor authentication support.
    /// Default: <c>false</c>.
    /// </summary>
    public bool EnableTwoFactor { get; init; }
}
