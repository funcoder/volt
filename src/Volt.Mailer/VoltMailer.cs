using FluentEmail.Core;
using Microsoft.Extensions.Options;

namespace Volt.Mailer;

/// <summary>
/// Base class for Volt mailers. Provides a fluent API for composing and sending emails.
/// Convention: Views/Mailers/{MailerName}/{MethodName}.cshtml is used as the Razor template.
/// Subclass this to define application-specific mailers with strongly-typed methods.
/// </summary>
/// <example>
/// <code>
/// public class UserMailer : VoltMailer
/// {
///     public UserMailer(IFluentEmail email, IOptions&lt;VoltMailerOptions&gt; options)
///         : base(email, options) { }
///
///     public async Task Welcome(User user)
///     {
///         To(user.Email);
///         Subject("Welcome aboard!");
///         await Send(new { Name = user.Name });
///     }
/// }
/// </code>
/// </example>
public abstract class VoltMailer
{
    private string? _to;
    private string? _from;
    private string? _subject;

    private readonly IFluentEmail _email;
    private readonly VoltMailerOptions _options;

    /// <summary>
    /// Initializes a new instance of the mailer with FluentEmail and configuration.
    /// </summary>
    /// <param name="email">The FluentEmail instance for composing messages.</param>
    /// <param name="options">The mailer configuration options.</param>
    protected VoltMailer(IFluentEmail email, IOptions<VoltMailerOptions> options)
    {
        _email = email ?? throw new ArgumentNullException(nameof(email));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Sets the recipient email address for this message.
    /// </summary>
    /// <param name="email">The recipient email address.</param>
    protected void To(string email) => _to = email;

    /// <summary>
    /// Overrides the default sender email address for this message.
    /// </summary>
    /// <param name="email">The sender email address.</param>
    protected void From(string email) => _from = email;

    /// <summary>
    /// Sets the subject line for this message.
    /// </summary>
    /// <param name="subject">The email subject.</param>
    protected void Subject(string subject) => _subject = subject;

    /// <summary>
    /// Renders the Razor template and sends the email. The template path is resolved
    /// by convention from the calling mailer class name and the method that set up the email.
    /// </summary>
    /// <typeparam name="TModel">The type of the view model passed to the Razor template.</typeparam>
    /// <param name="model">The view model to pass to the Razor template.</param>
    /// <exception cref="InvalidOperationException">Thrown when To or Subject has not been set.</exception>
    protected async Task Send<TModel>(TModel model)
    {
        if (string.IsNullOrEmpty(_to))
        {
            throw new InvalidOperationException(
                "Recipient address is required. Call To() before Send().");
        }

        if (string.IsNullOrEmpty(_subject))
        {
            throw new InvalidOperationException(
                "Subject is required. Call Subject() before Send().");
        }

        var templatePath = ResolveTemplatePath();

        var response = await _email
            .To(_to)
            .SetFrom(_from ?? _options.DefaultFrom)
            .Subject(_subject)
            .UsingTemplateFromFile(templatePath, model)
            .SendAsync();

        if (!response.Successful)
        {
            throw new InvalidOperationException(
                $"Failed to send email: {string.Join("; ", response.ErrorMessages)}");
        }

        ResetState();
    }

    /// <summary>
    /// Resolves the Razor template file path by convention:
    /// Views/Mailers/{MailerClassName}/{CallingMethodName}.cshtml
    /// </summary>
    private string ResolveTemplatePath()
    {
        var mailerName = GetType().Name;
        var callerName = ResolveCallerMethodName();
        return Path.Combine("Views", "Mailers", mailerName, $"{callerName}.cshtml");
    }

    /// <summary>
    /// Walks the call stack to find the public method on the mailer subclass that
    /// initiated the send, skipping base class methods.
    /// </summary>
    private string ResolveCallerMethodName()
    {
        var stackTrace = new System.Diagnostics.StackTrace();
        var mailerType = GetType();

        for (var i = 0; i < stackTrace.FrameCount; i++)
        {
            var method = stackTrace.GetFrame(i)?.GetMethod();
            if (method?.DeclaringType == mailerType && method.Name != nameof(Send))
            {
                return method.Name;
            }
        }

        return "Default";
    }

    /// <summary>
    /// Resets the per-message state so the mailer instance can be reused.
    /// </summary>
    private void ResetState()
    {
        _to = null;
        _from = null;
        _subject = null;
    }
}
