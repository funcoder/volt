namespace Volt.Mailer;

/// <summary>
/// Configuration options for the Volt mailer system.
/// Provides opinionated defaults for development with MailHog (localhost:1025)
/// and production-ready SMTP settings.
/// </summary>
public class VoltMailerOptions
{
    /// <summary>
    /// The default sender email address used when no explicit From is set.
    /// Default: <c>"noreply@myapp.com"</c>.
    /// </summary>
    public string DefaultFrom { get; set; } = "noreply@myapp.com";

    /// <summary>
    /// The SMTP server hostname.
    /// Default: <c>"localhost"</c>.
    /// </summary>
    public string SmtpHost { get; set; } = "localhost";

    /// <summary>
    /// The SMTP server port. Use 1025 for development (MailHog) or 587 for production.
    /// Default: <c>1025</c>.
    /// </summary>
    public int SmtpPort { get; set; } = 1025;

    /// <summary>
    /// Optional SMTP authentication username.
    /// </summary>
    public string? SmtpUsername { get; set; }

    /// <summary>
    /// Optional SMTP authentication password.
    /// </summary>
    public string? SmtpPassword { get; set; }

    /// <summary>
    /// Whether to use SSL/TLS for the SMTP connection.
    /// Default: <c>false</c>.
    /// </summary>
    public bool UseSsl { get; set; }

    /// <summary>
    /// Whether to enable the email preview UI at /volt/mailers in development.
    /// Default: <c>true</c>.
    /// </summary>
    public bool EnablePreview { get; set; } = true;
}
