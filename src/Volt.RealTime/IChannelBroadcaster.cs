namespace Volt.RealTime;

/// <summary>
/// Abstraction for broadcasting messages to real-time channels.
/// Inject this interface to send messages from anywhere in the application
/// (controllers, jobs, services) without a direct hub reference.
/// </summary>
public interface IChannelBroadcaster
{
    /// <summary>
    /// Broadcasts a message to all clients subscribed to the specified channel.
    /// </summary>
    /// <typeparam name="TChannel">The channel type to broadcast to.</typeparam>
    /// <param name="action">The action name clients are listening for.</param>
    /// <param name="data">The payload to send to clients.</param>
    Task Broadcast<TChannel>(string action, object data) where TChannel : VoltChannel;

    /// <summary>
    /// Broadcasts a message to a specific group within the specified channel.
    /// </summary>
    /// <typeparam name="TChannel">The channel type to broadcast to.</typeparam>
    /// <param name="group">The group name to target.</param>
    /// <param name="action">The action name clients are listening for.</param>
    /// <param name="data">The payload to send to clients.</param>
    Task BroadcastTo<TChannel>(string group, string action, object data) where TChannel : VoltChannel;
}
