using System.Collections.Concurrent;
using Volt.Core;

namespace Volt.Data.Callbacks;

/// <summary>
/// Shared utility for determining whether a CLR type is a Volt model.
/// Used by both <see cref="VoltDbContext"/> and <see cref="CallbackRunner"/>.
/// Results are cached per type so the hierarchy walk happens at most once.
/// </summary>
internal static class VoltModelHelper
{
    private static readonly ConcurrentDictionary<Type, bool> Cache = new();

    /// <summary>
    /// Returns <c>true</c> when <paramref name="type"/> inherits from
    /// <see cref="Model{T}"/> (at any depth in the hierarchy).
    /// </summary>
    internal static bool IsVoltModel(Type type)
    {
        return Cache.GetOrAdd(type, static t =>
        {
            var current = t;
            while (current is not null)
            {
                if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(Model<>))
                {
                    return true;
                }

                current = current.BaseType;
            }

            return false;
        });
    }
}
