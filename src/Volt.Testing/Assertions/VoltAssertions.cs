using System.Net;
using Volt.Core.Validation;
using Xunit;

namespace Volt.Testing.Assertions;

/// <summary>
/// Custom assertion helpers for Volt integration and unit tests.
/// Provides fluent assertion methods for models, HTTP responses, and flash messages.
/// </summary>
public static class VoltAssertions
{
    /// <summary>
    /// Asserts that the model passes all configured Volt validation rules.
    /// The model type must implement <see cref="IVoltValidatable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The model type that implements <see cref="IVoltValidatable{T}"/>.</typeparam>
    /// <param name="model">The model instance to validate.</param>
    public static void ShouldBeValid<T>(this T model) where T : class, IVoltValidatable<T>
    {
        var builder = new ValidationBuilder<T>();
        T.ConfigureValidations(builder);
        var result = builder.Validate(model);

        Assert.True(
            result.IsValid,
            $"Expected model to be valid, but found errors: {FormatErrors(result.Errors)}");
    }

    /// <summary>
    /// Asserts that the model fails validation on the specified property.
    /// The model type must implement <see cref="IVoltValidatable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The model type that implements <see cref="IVoltValidatable{T}"/>.</typeparam>
    /// <param name="model">The model instance to validate.</param>
    /// <param name="property">The property name expected to have a validation error.</param>
    public static void ShouldBeInvalid<T>(this T model, string property) where T : class, IVoltValidatable<T>
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(property);

        var builder = new ValidationBuilder<T>();
        T.ConfigureValidations(builder);
        var result = builder.Validate(model);

        Assert.False(result.IsValid, "Expected model to be invalid, but it passed validation.");

        var hasPropertyError = result.Errors.Exists(e =>
            string.Equals(e.PropertyName, property, StringComparison.OrdinalIgnoreCase));

        Assert.True(
            hasPropertyError,
            $"Expected validation error on '{property}', but errors were on: " +
            string.Join(", ", result.Errors.Select(e => e.PropertyName)));
    }

    /// <summary>
    /// Asserts that the HTTP response is a redirect to the specified path.
    /// </summary>
    /// <param name="response">The HTTP response to check.</param>
    /// <param name="path">The expected redirect path (relative or absolute).</param>
    public static void ShouldRedirectTo(this HttpResponseMessage response, string path)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        Assert.True(
            response.StatusCode is HttpStatusCode.Redirect or HttpStatusCode.MovedPermanently
                or HttpStatusCode.TemporaryRedirect or HttpStatusCode.PermanentRedirect,
            $"Expected a redirect status code, but got {(int)response.StatusCode} {response.StatusCode}.");

        var location = response.Headers.Location?.ToString() ?? string.Empty;
        Assert.EndsWith(path, location, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Asserts that the HTTP response has the specified status code.
    /// </summary>
    /// <param name="response">The HTTP response to check.</param>
    /// <param name="status">The expected HTTP status code.</param>
    public static void ShouldHaveStatus(this HttpResponseMessage response, HttpStatusCode status)
    {
        ArgumentNullException.ThrowIfNull(response);
        Assert.Equal(status, response.StatusCode);
    }

    /// <summary>
    /// Asserts that the HTTP response body contains the specified flash message text.
    /// Reads the response body and checks for the message substring.
    /// </summary>
    /// <param name="response">The HTTP response to check.</param>
    /// <param name="message">The flash message text expected in the response body.</param>
    public static async Task ShouldContainFlash(this HttpResponseMessage response, string message)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains(message, body, StringComparison.OrdinalIgnoreCase);
    }

    private static string FormatErrors(
        IEnumerable<FluentValidation.Results.ValidationFailure> errors)
    {
        return string.Join("; ", errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
    }
}
