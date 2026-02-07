using Volt.Data;

namespace Volt.Testing.Factories;

/// <summary>
/// Factory for building and persisting test model instances.
/// Inspired by FactoryBot in Rails, providing a clean API for creating
/// test data with sensible defaults and optional overrides.
/// </summary>
public static class Factory
{
    private static readonly Dictionary<Type, Delegate> _definitions = new();

    /// <summary>
    /// Defines a factory for a model type. The builder function returns
    /// a new instance with default property values.
    /// </summary>
    /// <typeparam name="T">The model type to define a factory for.</typeparam>
    /// <param name="builder">A function that creates a default instance.</param>
    public static void Define<T>(Func<T> builder) where T : class
    {
        ArgumentNullException.ThrowIfNull(builder);
        _definitions[typeof(T)] = builder;
    }

    /// <summary>
    /// Builds an instance in memory without persisting to the database.
    /// </summary>
    /// <typeparam name="T">The model type to build.</typeparam>
    /// <param name="customize">Optional action to override default property values.</param>
    /// <returns>A new instance of <typeparamref name="T"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no factory is defined for <typeparamref name="T"/>.</exception>
    public static T Build<T>(Action<T>? customize = null) where T : class
    {
        var instance = CreateInstance<T>();
        customize?.Invoke(instance);
        return instance;
    }

    /// <summary>
    /// Builds and persists an instance to the database.
    /// </summary>
    /// <typeparam name="T">The model type to create.</typeparam>
    /// <param name="db">The database context to persist to.</param>
    /// <param name="customize">Optional action to override default property values.</param>
    /// <returns>The persisted instance of <typeparamref name="T"/>.</returns>
    public static async Task<T> Create<T>(VoltDbContext db, Action<T>? customize = null) where T : class
    {
        ArgumentNullException.ThrowIfNull(db);

        var instance = Build(customize);
        db.Set<T>().Add(instance);
        await db.SaveChangesAsync();
        return instance;
    }

    /// <summary>
    /// Builds a list of instances in memory without persisting.
    /// </summary>
    /// <typeparam name="T">The model type to build.</typeparam>
    /// <param name="count">The number of instances to build.</param>
    /// <param name="customize">Optional action to customize each instance. Receives the instance and its zero-based index.</param>
    /// <returns>A read-only list of <typeparamref name="T"/> instances.</returns>
    public static IReadOnlyList<T> BuildList<T>(int count, Action<T, int>? customize = null) where T : class
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

        var items = new List<T>(count);

        for (var i = 0; i < count; i++)
        {
            var instance = CreateInstance<T>();
            customize?.Invoke(instance, i);
            items.Add(instance);
        }

        return items.AsReadOnly();
    }

    /// <summary>
    /// Builds and persists a list of instances to the database.
    /// </summary>
    /// <typeparam name="T">The model type to create.</typeparam>
    /// <param name="db">The database context to persist to.</param>
    /// <param name="count">The number of instances to create.</param>
    /// <param name="customize">Optional action to customize each instance. Receives the instance and its zero-based index.</param>
    /// <returns>A read-only list of persisted <typeparamref name="T"/> instances.</returns>
    public static async Task<IReadOnlyList<T>> CreateList<T>(
        VoltDbContext db,
        int count,
        Action<T, int>? customize = null) where T : class
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

        var items = new List<T>(count);

        for (var i = 0; i < count; i++)
        {
            var instance = CreateInstance<T>();
            customize?.Invoke(instance, i);
            items.Add(instance);
        }

        db.Set<T>().AddRange(items);
        await db.SaveChangesAsync();
        return items.AsReadOnly();
    }

    /// <summary>
    /// Removes all registered factory definitions. Useful for test cleanup.
    /// </summary>
    public static void Reset() => _definitions.Clear();

    private static T CreateInstance<T>() where T : class
    {
        if (!_definitions.TryGetValue(typeof(T), out var definition))
        {
            throw new InvalidOperationException(
                $"No factory defined for {typeof(T).Name}. Call Factory.Define<{typeof(T).Name}>() first.");
        }

        return ((Func<T>)definition)();
    }
}
