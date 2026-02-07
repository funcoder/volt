namespace Volt.Jobs;

/// <summary>
/// Abstraction for enqueueing and scheduling background jobs.
/// Provides a Rails-like interface for dispatching work asynchronously.
/// </summary>
public interface IJobQueue
{
    /// <summary>
    /// Enqueues a job for immediate background execution with the given payload.
    /// </summary>
    /// <typeparam name="TJob">The job type to enqueue.</typeparam>
    /// <typeparam name="TPayload">The payload type for the job.</typeparam>
    /// <param name="payload">The data to pass to the job.</param>
    void Enqueue<TJob, TPayload>(TPayload payload)
        where TJob : VoltJob<TPayload>
        where TPayload : class;

    /// <summary>
    /// Schedules a job for background execution after the specified delay.
    /// </summary>
    /// <typeparam name="TJob">The job type to schedule.</typeparam>
    /// <typeparam name="TPayload">The payload type for the job.</typeparam>
    /// <param name="payload">The data to pass to the job.</param>
    /// <param name="delay">The time to wait before executing the job.</param>
    void Schedule<TJob, TPayload>(TPayload payload, TimeSpan delay)
        where TJob : VoltJob<TPayload>
        where TPayload : class;
}
