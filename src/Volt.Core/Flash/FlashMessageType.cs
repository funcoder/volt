namespace Volt.Core.Flash;

/// <summary>
/// The severity or category of a flash message displayed to the user.
/// </summary>
public enum FlashMessageType
{
    /// <summary>Indicates a successful operation.</summary>
    Success,

    /// <summary>Indicates a warning that the user should be aware of.</summary>
    Warning,

    /// <summary>Indicates an error or failure.</summary>
    Error,

    /// <summary>Indicates a general informational message.</summary>
    Info
}
