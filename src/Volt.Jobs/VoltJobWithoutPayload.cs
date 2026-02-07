using Coravel.Invocable;

namespace Volt.Jobs;

/// <summary>
/// Base class for Volt background jobs that do not require a payload.
/// Implement <see cref="Execute"/> to define the job logic.
/// Jobs are auto-discovered and registered in DI by convention.
/// </summary>
public abstract class VoltJobWithoutPayload : IInvocable
{
    /// <summary>
    /// Invoked by the Coravel queue/scheduler. Delegates to <see cref="Execute"/>.
    /// </summary>
    public async Task Invoke() => await Execute();

    /// <summary>
    /// Implement this method to define the job's work.
    /// </summary>
    public abstract Task Execute();
}
