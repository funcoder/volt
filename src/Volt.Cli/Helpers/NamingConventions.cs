namespace Volt.Cli.Helpers;

/// <summary>
/// Shared naming convention utilities for code generation and destruction.
/// Provides PascalCase enforcement, suffix management, pluralization, and snake_case conversion.
/// </summary>
public static class NamingConventions
{
    public static string EnsurePascalCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        return char.ToUpperInvariant(name[0]) + name[1..];
    }

    public static string EnsureControllerSuffix(string name)
    {
        var pascalName = EnsurePascalCase(name);
        return pascalName.EndsWith("Controller", StringComparison.Ordinal)
            ? pascalName
            : $"{pascalName}Controller";
    }

    public static string EnsureMailerSuffix(string name)
    {
        var pascalName = EnsurePascalCase(name);
        return pascalName.EndsWith("Mailer", StringComparison.Ordinal)
            ? pascalName
            : $"{pascalName}Mailer";
    }

    public static string EnsureChannelSuffix(string name)
    {
        var pascalName = EnsurePascalCase(name);
        return pascalName.EndsWith("Channel", StringComparison.Ordinal)
            ? pascalName
            : $"{pascalName}Channel";
    }

    public static string ToRoutePath(string modelName) => ToTableName(modelName);

    public static string ToTableName(string modelName) => Pluralize(ToSnakeCase(modelName));

    public static string Pluralize(string singular)
    {
        if (string.IsNullOrEmpty(singular)) return singular;

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

    public static string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        var result = new System.Text.StringBuilder();
        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (char.IsUpper(c) && i > 0)
            {
                result.Append('_');
            }
            result.Append(char.ToLowerInvariant(c));
        }
        return result.ToString();
    }

    public static string GetDefaultTestValue(string type) => type switch
    {
        "string" => "\"Test\"",
        "int" => "1",
        "bool" => "true",
        "decimal" => "1.0m",
        "DateTime" => "DateTime.UtcNow",
        _ => "default",
    };

    private static bool IsVowel(char c) => c is 'a' or 'e' or 'i' or 'o' or 'u';
}
