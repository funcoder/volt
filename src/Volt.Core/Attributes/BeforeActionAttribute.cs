namespace Volt.Core.Attributes;

/// <summary>
/// Specifies a controller method to be executed before the designated actions.
/// When no actions are specified, the filter applies to all actions in the controller.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class BeforeActionAttribute : Attribute
{
    /// <summary>
    /// The name of the filter method to invoke before the action.
    /// </summary>
    public string MethodName { get; }

    /// <summary>
    /// The action names this filter applies to. An empty array means all actions.
    /// </summary>
    public string[] Actions { get; }

    /// <summary>
    /// Creates a new before-action filter.
    /// </summary>
    /// <param name="methodName">The name of the filter method to invoke.</param>
    /// <param name="actions">The action names to apply this filter to. Empty means all actions.</param>
    public BeforeActionAttribute(string methodName, params string[] actions)
    {
        MethodName = methodName;
        Actions = actions;
    }
}
