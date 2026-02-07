using Microsoft.AspNetCore.SignalR;

namespace Volt.RealTime;

/// <summary>
/// Base class for Volt real-time channels. Channels are auto-discovered
/// and mapped to SignalR hubs by convention.
/// Convention: Channels/ChatChannel.cs maps to /volt/channels/chat
/// </summary>
public abstract class VoltChannel : Hub
{
    /// <summary>
    /// Called when a client subscribes to this channel.
    /// Override to handle connection logic such as joining groups or sending initial state.
    /// </summary>
    /// <param name="connectionId">The unique identifier for the connected client.</param>
    public virtual Task OnSubscribed(string connectionId) => Task.CompletedTask;

    /// <summary>
    /// Called when a client unsubscribes from this channel.
    /// Override to handle disconnection logic such as leaving groups or cleanup.
    /// </summary>
    /// <param name="connectionId">The unique identifier for the disconnected client.</param>
    public virtual Task OnUnsubscribed(string connectionId) => Task.CompletedTask;

    /// <inheritdoc />
    public override async Task OnConnectedAsync()
    {
        await OnSubscribed(Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    /// <inheritdoc />
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await OnUnsubscribed(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}
