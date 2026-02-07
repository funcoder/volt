using System.Linq.Expressions;
using FluentValidation;
using FluentValidation.Results;

namespace Volt.Core.Validation;

/// <summary>
/// Fluent validation builder for Volt models.
/// Wraps FluentValidation with a concise, Rails-like API for defining validation rules.
/// </summary>
/// <typeparam name="T">The model type to validate.</typeparam>
public sealed class ValidationBuilder<T> where T : class
{
    private readonly InternalValidator _validator = new();

    /// <summary>
    /// Begins defining validation rules for a property.
    /// </summary>
    /// <typeparam name="TProp">The property type.</typeparam>
    /// <param name="expression">An expression selecting the property to validate.</param>
    /// <returns>A chainable <see cref="PropertyRuleBuilder{T, TProp}"/> for defining rules.</returns>
    public PropertyRuleBuilder<T, TProp> Property<TProp>(Expression<Func<T, TProp>> expression)
    {
        return new PropertyRuleBuilder<T, TProp>(_validator, expression);
    }

    /// <summary>
    /// Validates the given instance against all configured rules.
    /// </summary>
    /// <param name="instance">The model instance to validate.</param>
    /// <returns>The validation result containing any failures.</returns>
    public ValidationResult Validate(T instance)
    {
        return _validator.Validate(instance);
    }

    /// <summary>
    /// Asynchronously validates the given instance against all configured rules.
    /// </summary>
    /// <param name="instance">The model instance to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The validation result containing any failures.</returns>
    public Task<ValidationResult> ValidateAsync(T instance, CancellationToken cancellationToken = default)
    {
        return _validator.ValidateAsync(instance, cancellationToken);
    }

    /// <summary>
    /// Builds and returns the underlying FluentValidation validator.
    /// </summary>
    internal IValidator<T> Build() => _validator;

    private sealed class InternalValidator : AbstractValidator<T>;
}
