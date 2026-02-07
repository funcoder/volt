using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace Volt.RealTime;

/// <summary>
/// Default implementation of <see cref="IChannelBroadcaster"/> backed by SignalR's
/// <see cref="IHubContext{THub}"/>. Resolves the appropriate hub context from DI
/// to broadcast messages to connected clients.
/// </summary>
public sealed class VoltChannelBroadcaster : IChannelBroadcaster
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Creates a new <see cref="VoltChannelBroadcaster"/>.
    /// </summary>
    /// <param name="serviceProvider">The DI service provider for resolving hub contexts.</param>
    public VoltChannelBroadcaster(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public async Task Broadcast<TChannel>(string action, object data) where TChannel : VoltChannel
    {
        var hubContext = _serviceProvider.GetRequiredService<IHubContext<TChannel>>();
        await hubContext.Clients.All.SendAsync(action, data);
    }

    /// <inheritdoc />
    public async Task BroadcastTo<TChannel>(string group, string action, object data) where TChannel : VoltChannel
    {
        var hubContext = _serviceProvider.GetRequiredService<IHubContext<TChannel>>();
        await hubContext.Clients.Group(group).SendAsync(action, data);
    }
}
