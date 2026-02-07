namespace Volt.Core.Flash;

/// <summary>
/// An immutable flash message record for transient user notifications.
/// Flash messages are displayed once and then discarded.
/// </summary>
/// <param name="Type">The severity or category of the message.</param>
/// <param name="Message">The message text to display to the user.</param>
public sealed record FlashMessage(FlashMessageType Type, string Message);
