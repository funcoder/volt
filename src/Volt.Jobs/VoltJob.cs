using Coravel.Invocable;

namespace Volt.Jobs;

/// <summary>
/// Base class for Volt background jobs with a typed payload.
/// Implement <see cref="Execute"/> to define the job logic.
/// Jobs are auto-discovered and registered in DI by convention.
/// </summary>
/// <typeparam name="TPayload">The type of the payload passed to the job.</typeparam>
public abstract class VoltJob<TPayload> : IInvocable, IInvocableWithPayload<TPayload> where TPayload : class
{
    /// <summary>
    /// The payload to be processed by this job.
    /// Set by the queue infrastructure before invocation.
    /// </summary>
    public TPayload Payload { get; set; } = default!;

    /// <summary>
    /// Invoked by the Coravel queue/scheduler. Validates the payload and delegates to <see cref="Execute"/>.
    /// </summary>
    public async Task Invoke()
    {
        if (Payload is null)
        {
            throw new InvalidOperationException(
                $"Job payload was not set for {GetType().Name}. " +
                "Enqueue jobs through IJobQueue rather than invoking directly.");
        }

        await Execute(Payload);
    }

    /// <summary>
    /// Implement this method to define the job's work.
    /// </summary>
    /// <param name="payload">The strongly-typed payload for this job invocation.</param>
    public abstract Task Execute(TPayload payload);
}
