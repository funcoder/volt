using Coravel.Invocable;
using Coravel.Scheduling.Schedule.Interfaces;

namespace Volt.Jobs.Scheduling;

/// <summary>
/// Fluent configuration API for scheduling recurring background jobs.
/// Wraps Coravel's <see cref="IScheduler"/> with Rails-like convenience methods.
/// </summary>
public sealed class VoltSchedulerConfig
{
    private readonly IScheduler _scheduler;

    /// <summary>
    /// Creates a new scheduler configuration wrapping the given Coravel scheduler.
    /// </summary>
    /// <param name="scheduler">The Coravel scheduler instance.</param>
    public VoltSchedulerConfig(IScheduler scheduler)
    {
        _scheduler = scheduler;
    }

    /// <summary>
    /// Schedules a job to run every minute.
    /// </summary>
    /// <typeparam name="TJob">The job type to schedule. Must implement <see cref="IInvocable"/>.</typeparam>
    /// <returns>This config instance for chaining.</returns>
    public VoltSchedulerConfig EveryMinute<TJob>() where TJob : IInvocable
    {
        _scheduler.Schedule<TJob>().EveryMinute();
        return this;
    }

    /// <summary>
    /// Schedules a job to run every hour.
    /// </summary>
    /// <typeparam name="TJob">The job type to schedule. Must implement <see cref="IInvocable"/>.</typeparam>
    /// <returns>This config instance for chaining.</returns>
    public VoltSchedulerConfig Hourly<TJob>() where TJob : IInvocable
    {
        _scheduler.Schedule<TJob>().Hourly();
        return this;
    }

    /// <summary>
    /// Schedules a job to run once daily at midnight.
    /// </summary>
    /// <typeparam name="TJob">The job type to schedule. Must implement <see cref="IInvocable"/>.</typeparam>
    /// <returns>This config instance for chaining.</returns>
    public VoltSchedulerConfig Daily<TJob>() where TJob : IInvocable
    {
        _scheduler.Schedule<TJob>().Daily();
        return this;
    }

    /// <summary>
    /// Schedules a job to run once weekly on Monday at midnight.
    /// </summary>
    /// <typeparam name="TJob">The job type to schedule. Must implement <see cref="IInvocable"/>.</typeparam>
    /// <returns>This config instance for chaining.</returns>
    public VoltSchedulerConfig Weekly<TJob>() where TJob : IInvocable
    {
        _scheduler.Schedule<TJob>().Weekly();
        return this;
    }

    /// <summary>
    /// Schedules a job using a custom cron expression.
    /// </summary>
    /// <typeparam name="TJob">The job type to schedule. Must implement <see cref="IInvocable"/>.</typeparam>
    /// <param name="cronExpression">A standard cron expression (e.g., "0 */6 * * *" for every 6 hours).</param>
    /// <returns>This config instance for chaining.</returns>
    public VoltSchedulerConfig Cron<TJob>(string cronExpression) where TJob : IInvocable
    {
        if (string.IsNullOrWhiteSpace(cronExpression))
        {
            throw new ArgumentException("Cron expression cannot be null or empty.", nameof(cronExpression));
        }

        _scheduler.Schedule<TJob>().Cron(cronExpression);
        return this;
    }
}
