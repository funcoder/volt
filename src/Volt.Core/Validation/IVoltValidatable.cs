namespace Volt.Core.Validation;

/// <summary>
/// Interface for models that define validation rules.
/// Implement this interface to configure validations using the fluent <see cref="ValidationBuilder{T}"/>.
/// </summary>
/// <typeparam name="T">The model type being validated.</typeparam>
public interface IVoltValidatable<T> where T : class
{
    /// <summary>
    /// Configures validation rules for this model using the provided builder.
    /// </summary>
    /// <param name="builder">The validation builder used to define rules.</param>
    static abstract void ConfigureValidations(ValidationBuilder<T> builder);
}
