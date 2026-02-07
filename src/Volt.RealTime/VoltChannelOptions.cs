namespace Volt.RealTime;

/// <summary>
/// Configuration options for the Volt real-time channel infrastructure.
/// Used to customize hub routing, timeouts, and debugging behavior.
/// </summary>
public sealed class VoltChannelOptions
{
    /// <summary>
    /// The base path prefix for all channel hub routes.
    /// Default: "/volt/channels".
    /// </summary>
    public string BasePath { get; init; } = "/volt/channels";

    /// <summary>
    /// Whether to include detailed error messages in hub responses.
    /// Should only be enabled in development environments.
    /// Default: false.
    /// </summary>
    public bool EnableDetailedErrors { get; init; }

    /// <summary>
    /// The interval at which the server sends keep-alive pings to connected clients.
    /// Default: 15 seconds.
    /// </summary>
    public TimeSpan KeepAliveInterval { get; init; } = TimeSpan.FromSeconds(15);

    /// <summary>
    /// The maximum time the server waits for a client response before considering
    /// the connection lost.
    /// Default: 30 seconds.
    /// </summary>
    public TimeSpan ClientTimeoutInterval { get; init; } = TimeSpan.FromSeconds(30);
}
