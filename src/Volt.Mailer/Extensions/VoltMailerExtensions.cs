using System.Reflection;
using FluentEmail.MailKitSmtp;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Volt.Mailer.Preview;

namespace Volt.Mailer.Extensions;

/// <summary>
/// Extension methods for registering Volt mailer services and preview middleware.
/// </summary>
public static class VoltMailerExtensions
{
    /// <summary>
    /// Registers Volt mailer services including FluentEmail with MailKit sender
    /// and Razor template renderer. Discovers and registers all mailer subclasses
    /// from the calling assembly.
    /// </summary>
    /// <param name="services">The service collection to add mailer services to.</param>
    /// <param name="configure">Optional configuration action for <see cref="VoltMailerOptions"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddVoltMailer(
        this IServiceCollection services,
        Action<VoltMailerOptions>? configure = null)
    {
        var options = new VoltMailerOptions();
        configure?.Invoke(options);

        services.Configure<VoltMailerOptions>(opt =>
        {
            opt.DefaultFrom = options.DefaultFrom;
            opt.SmtpHost = options.SmtpHost;
            opt.SmtpPort = options.SmtpPort;
            opt.SmtpUsername = options.SmtpUsername;
            opt.SmtpPassword = options.SmtpPassword;
            opt.UseSsl = options.UseSsl;
            opt.EnablePreview = options.EnablePreview;
        });

        var smtpSettings = new SmtpClientOptions
        {
            Server = options.SmtpHost,
            Port = options.SmtpPort,
            UseSsl = options.UseSsl,
            RequiresAuthentication = !string.IsNullOrEmpty(options.SmtpUsername),
            User = options.SmtpUsername,
            Password = options.SmtpPassword
        };

        services
            .AddFluentEmail(options.DefaultFrom)
            .AddRazorRenderer()
            .AddMailKitSender(smtpSettings);

        RegisterMailerSubclasses(services, Assembly.GetCallingAssembly());

        return services;
    }

    /// <summary>
    /// Maps the Volt mailer preview endpoints at /volt/mailers for browsing
    /// email templates in development. Only active when the hosting environment
    /// is Development.
    /// </summary>
    /// <param name="app">The web application to add preview routes to.</param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication UseVoltMailerPreview(this WebApplication app)
    {
        var env = app.Services.GetRequiredService<IWebHostEnvironment>();

        if (env.IsDevelopment())
        {
            MailerPreviewEndpoints.Map(app);
        }

        return app;
    }

    /// <summary>
    /// Discovers all non-abstract subclasses of <see cref="VoltMailer"/> in the
    /// given assembly and registers them as transient services.
    /// </summary>
    private static void RegisterMailerSubclasses(
        IServiceCollection services,
        Assembly assembly)
    {
        var mailerTypes = assembly
            .GetTypes()
            .Where(t => t is { IsAbstract: false, IsClass: true }
                        && t.IsSubclassOf(typeof(VoltMailer)));

        foreach (var mailerType in mailerTypes)
        {
            services.AddTransient(mailerType);
        }
    }
}
