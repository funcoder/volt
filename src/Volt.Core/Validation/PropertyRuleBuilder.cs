using System.Linq.Expressions;
using FluentValidation;

namespace Volt.Core.Validation;

/// <summary>
/// Chainable validation rule builder for a single property.
/// Wraps FluentValidation rules with a concise, Rails-like API.
/// </summary>
/// <typeparam name="T">The model type.</typeparam>
/// <typeparam name="TProp">The property type.</typeparam>
public sealed class PropertyRuleBuilder<T, TProp> where T : class
{
    private readonly AbstractValidator<T> _validator;
    private readonly Expression<Func<T, TProp>> _expression;

    internal PropertyRuleBuilder(AbstractValidator<T> validator, Expression<Func<T, TProp>> expression)
    {
        _validator = validator;
        _expression = expression;
    }

    /// <summary>
    /// Marks the property as required (not null or empty).
    /// </summary>
    public PropertyRuleBuilder<T, TProp> Required()
    {
        _validator.RuleFor(_expression).NotEmpty();
        return this;
    }

    /// <summary>
    /// Sets the maximum length for a string property.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public PropertyRuleBuilder<T, TProp> MaxLength(int max)
    {
        if (_expression is Expression<Func<T, string>> stringExpr)
        {
            _validator.RuleFor(stringExpr).MaximumLength(max);
        }

        return this;
    }

    /// <summary>
    /// Sets the minimum length for a string property.
    /// </summary>
    /// <param name="min">The minimum required length.</param>
    public PropertyRuleBuilder<T, TProp> MinLength(int min)
    {
        if (_expression is Expression<Func<T, string>> stringExpr)
        {
            _validator.RuleFor(stringExpr).MinimumLength(min);
        }

        return this;
    }

    /// <summary>
    /// Validates that a string property is a valid email address.
    /// </summary>
    public PropertyRuleBuilder<T, TProp> Email()
    {
        if (_expression is Expression<Func<T, string>> stringExpr)
        {
            _validator.RuleFor(stringExpr).EmailAddress();
        }

        return this;
    }

    /// <summary>
    /// Validates that a string property matches the specified regular expression pattern.
    /// </summary>
    /// <param name="pattern">The regex pattern to match against.</param>
    public PropertyRuleBuilder<T, TProp> Matches(string pattern)
    {
        if (_expression is Expression<Func<T, string>> stringExpr)
        {
            _validator.RuleFor(stringExpr).Matches(pattern);
        }

        return this;
    }

    /// <summary>
    /// Validates that a comparable property is greater than the specified value.
    /// Requires that <typeparamref name="TProp"/> implements <see cref="IComparable{TProp}"/>.
    /// </summary>
    /// <param name="value">The exclusive lower bound.</param>
    public PropertyRuleBuilder<T, TProp> GreaterThan(TProp value)
    {
        _validator.RuleFor(_expression).Must(actual =>
            actual is IComparable<TProp> comparable && comparable.CompareTo(value) > 0)
            .WithMessage($"'{{PropertyName}}' must be greater than '{value}'.");
        return this;
    }

    /// <summary>
    /// Validates that a comparable property is less than the specified value.
    /// Requires that <typeparamref name="TProp"/> implements <see cref="IComparable{TProp}"/>.
    /// </summary>
    /// <param name="value">The exclusive upper bound.</param>
    public PropertyRuleBuilder<T, TProp> LessThan(TProp value)
    {
        _validator.RuleFor(_expression).Must(actual =>
            actual is IComparable<TProp> comparable && comparable.CompareTo(value) < 0)
            .WithMessage($"'{{PropertyName}}' must be less than '{value}'.");
        return this;
    }
}
