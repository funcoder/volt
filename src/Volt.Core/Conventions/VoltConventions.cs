using System.Text.RegularExpressions;

namespace Volt.Core.Conventions;

/// <summary>
/// Static class holding convention defaults for the Volt framework.
/// Provides Rails-like naming conventions for tables, columns, and keys.
/// </summary>
public static partial class VoltConventions
{
    /// <summary>
    /// Converts a model class name to a table name using snake_case pluralization.
    /// Example: "UserProfile" becomes "user_profiles".
    /// </summary>
    public static string ToTableName(string modelName)
    {
        var snaked = ToSnakeCase(modelName);
        return Pluralize(snaked);
    }

    /// <summary>
    /// Converts a property name to a column name using snake_case.
    /// Example: "FirstName" becomes "first_name".
    /// </summary>
    public static string ToColumnName(string propertyName)
    {
        return ToSnakeCase(propertyName);
    }

    /// <summary>
    /// Generates a foreign key column name from a model name.
    /// Example: "User" becomes "user_id".
    /// </summary>
    public static string ToForeignKey(string modelName)
    {
        return $"{ToSnakeCase(modelName)}_id";
    }

    /// <summary>
    /// Generates a join table name from two model names (alphabetically ordered).
    /// Example: ("Tag", "Post") becomes "posts_tags".
    /// </summary>
    public static string ToJoinTableName(string modelNameA, string modelNameB)
    {
        var names = new[] { Pluralize(ToSnakeCase(modelNameA)), Pluralize(ToSnakeCase(modelNameB)) };
        Array.Sort(names, StringComparer.Ordinal);
        return string.Join("_", names);
    }

    /// <summary>
    /// Converts a PascalCase string to snake_case.
    /// </summary>
    public static string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        return PascalCaseBoundary().Replace(input, "$1_$2").ToLowerInvariant();
    }

    /// <summary>
    /// Naive pluralization: adds "s" or "es" as appropriate.
    /// Handles common English patterns but not irregular plurals.
    /// </summary>
    public static string Pluralize(string singular)
    {
        if (string.IsNullOrEmpty(singular))
        {
            return singular;
        }

        if (singular.EndsWith("s", StringComparison.Ordinal)
            || singular.EndsWith("x", StringComparison.Ordinal)
            || singular.EndsWith("z", StringComparison.Ordinal)
            || singular.EndsWith("sh", StringComparison.Ordinal)
            || singular.EndsWith("ch", StringComparison.Ordinal))
        {
            return $"{singular}es";
        }

        if (singular.EndsWith('y') && singular.Length > 1 && !IsVowel(singular[^2]))
        {
            return $"{singular[..^1]}ies";
        }

        return $"{singular}s";
    }

    private static bool IsVowel(char c) =>
        c is 'a' or 'e' or 'i' or 'o' or 'u';

    [GeneratedRegex("([a-z0-9])([A-Z])")]
    private static partial Regex PascalCaseBoundary();
}
