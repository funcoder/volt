using Coravel.Queuing.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Volt.Jobs;

/// <summary>
/// Default implementation of <see cref="IJobQueue"/> backed by Coravel's queue infrastructure.
/// Resolves jobs from DI, sets the payload, and enqueues them for execution.
/// </summary>
public sealed class VoltJobQueue : IJobQueue
{
    private readonly IQueue _queue;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Creates a new <see cref="VoltJobQueue"/>.
    /// </summary>
    /// <param name="queue">The Coravel queue instance.</param>
    /// <param name="serviceProvider">The DI service provider for resolving job instances.</param>
    public VoltJobQueue(IQueue queue, IServiceProvider serviceProvider)
    {
        _queue = queue;
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public void Enqueue<TJob, TPayload>(TPayload payload)
        where TJob : VoltJob<TPayload>
        where TPayload : class
    {
        _queue.QueueInvocableWithPayload<TJob, TPayload>(payload);
    }

    /// <inheritdoc />
    public void Schedule<TJob, TPayload>(TPayload payload, TimeSpan delay)
        where TJob : VoltJob<TPayload>
        where TPayload : class
    {
        _ = Task.Run(async () =>
        {
            await Task.Delay(delay);

            using var scope = _serviceProvider.CreateScope();
            var job = scope.ServiceProvider.GetRequiredService<TJob>();
            job.Payload = payload;
            await job.Invoke();
        });
    }
}
