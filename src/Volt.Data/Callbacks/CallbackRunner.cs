using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Volt.Core.Callbacks;

namespace Volt.Data.Callbacks;

/// <summary>
/// Flags indicating which callback interfaces a model type implements.
/// Cached per type so the reflection cost is paid only once.
/// </summary>
[Flags]
internal enum CallbackFlags
{
    None = 0,
    BeforeSave = 1 << 0,
    AfterSave = 1 << 1,
    BeforeCreate = 1 << 2,
    AfterCreate = 1 << 3,
    BeforeUpdate = 1 << 4,
    AfterUpdate = 1 << 5,
    BeforeDestroy = 1 << 6,
    AfterDestroy = 1 << 7,
}

/// <summary>
/// Discovers and invokes lifecycle callbacks on tracked Volt model entities.
/// Uses a static <see cref="ConcurrentDictionary{TKey,TValue}"/> to cache
/// interface checks so reflection is performed at most once per model type.
/// </summary>
internal sealed class CallbackRunner
{
    private static readonly ConcurrentDictionary<Type, CallbackFlags> FlagCache = new();

    /// <summary>
    /// Runs all "before" callbacks for tracked entities whose state matches.
    /// Execution order per entity: BeforeSave then BeforeCreate / BeforeUpdate / BeforeDestroy.
    /// </summary>
    internal async Task RunBeforeCallbacksAsync(
        ChangeTracker changeTracker,
        CancellationToken cancellationToken)
    {
        foreach (var entry in changeTracker.Entries().ToList())
        {
            if (!VoltModelHelper.IsVoltModel(entry.Entity.GetType()))
            {
                continue;
            }

            var flags = GetFlags(entry.Entity.GetType());
            if (flags == CallbackFlags.None)
            {
                continue;
            }

            var entity = entry.Entity;

            switch (entry.State)
            {
                case EntityState.Added:
                    if (flags.HasFlag(CallbackFlags.BeforeSave))
                        await ((IBeforeSave)entity).BeforeSaveAsync(cancellationToken);
                    if (flags.HasFlag(CallbackFlags.BeforeCreate))
                        await ((IBeforeCreate)entity).BeforeCreateAsync(cancellationToken);
                    break;

                case EntityState.Modified:
                    if (flags.HasFlag(CallbackFlags.BeforeSave))
                        await ((IBeforeSave)entity).BeforeSaveAsync(cancellationToken);
                    if (flags.HasFlag(CallbackFlags.BeforeUpdate))
                        await ((IBeforeUpdate)entity).BeforeUpdateAsync(cancellationToken);
                    break;

                case EntityState.Deleted:
                    if (flags.HasFlag(CallbackFlags.BeforeDestroy))
                        await ((IBeforeDestroy)entity).BeforeDestroyAsync(cancellationToken);
                    break;
            }
        }
    }

    /// <summary>
    /// Runs all "after" callbacks for tracked entities whose state matches.
    /// Execution order per entity: AfterCreate / AfterUpdate / AfterDestroy then AfterSave.
    /// </summary>
    internal async Task RunAfterCallbacksAsync(
        IReadOnlyList<(object Entity, EntityState State)> trackedEntities,
        CancellationToken cancellationToken)
    {
        foreach (var (entity, state) in trackedEntities)
        {
            var flags = GetFlags(entity.GetType());
            if (flags == CallbackFlags.None)
            {
                continue;
            }

            switch (state)
            {
                case EntityState.Added:
                    if (flags.HasFlag(CallbackFlags.AfterCreate))
                        await ((IAfterCreate)entity).AfterCreateAsync(cancellationToken);
                    if (flags.HasFlag(CallbackFlags.AfterSave))
                        await ((IAfterSave)entity).AfterSaveAsync(cancellationToken);
                    break;

                case EntityState.Modified:
                    if (flags.HasFlag(CallbackFlags.AfterUpdate))
                        await ((IAfterUpdate)entity).AfterUpdateAsync(cancellationToken);
                    if (flags.HasFlag(CallbackFlags.AfterSave))
                        await ((IAfterSave)entity).AfterSaveAsync(cancellationToken);
                    break;

                case EntityState.Deleted:
                    if (flags.HasFlag(CallbackFlags.AfterDestroy))
                        await ((IAfterDestroy)entity).AfterDestroyAsync(cancellationToken);
                    break;
            }
        }
    }

    /// <summary>
    /// Snapshots tracked entities and their states before <c>SaveChanges</c> runs,
    /// so "after" callbacks can reference the original state even though EF Core
    /// resets entries to <see cref="EntityState.Unchanged"/> after a successful save.
    /// </summary>
    internal static List<(object Entity, EntityState State)> SnapshotTrackedEntities(
        ChangeTracker changeTracker)
    {
        return changeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Where(e => VoltModelHelper.IsVoltModel(e.Entity.GetType()))
            .Select(e => (e.Entity, e.State))
            .ToList();
    }

    private static CallbackFlags GetFlags(Type type)
    {
        return FlagCache.GetOrAdd(type, static t =>
        {
            var flags = CallbackFlags.None;

            if (typeof(IBeforeSave).IsAssignableFrom(t)) flags |= CallbackFlags.BeforeSave;
            if (typeof(IAfterSave).IsAssignableFrom(t)) flags |= CallbackFlags.AfterSave;
            if (typeof(IBeforeCreate).IsAssignableFrom(t)) flags |= CallbackFlags.BeforeCreate;
            if (typeof(IAfterCreate).IsAssignableFrom(t)) flags |= CallbackFlags.AfterCreate;
            if (typeof(IBeforeUpdate).IsAssignableFrom(t)) flags |= CallbackFlags.BeforeUpdate;
            if (typeof(IAfterUpdate).IsAssignableFrom(t)) flags |= CallbackFlags.AfterUpdate;
            if (typeof(IBeforeDestroy).IsAssignableFrom(t)) flags |= CallbackFlags.BeforeDestroy;
            if (typeof(IAfterDestroy).IsAssignableFrom(t)) flags |= CallbackFlags.AfterDestroy;

            return flags;
        });
    }
}
