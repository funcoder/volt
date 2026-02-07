namespace Volt.Cli.Helpers;

/// <summary>
/// Parses field definitions from the command line in "name:type" format
/// and maps them to C# types for code generation.
/// </summary>
public static class FieldParser
{
    private static readonly IReadOnlyDictionary<string, string> TypeMappings =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["string"] = "string",
            ["int"] = "int",
            ["bool"] = "bool",
            ["text"] = "string",
            ["decimal"] = "decimal",
            ["datetime"] = "DateTime",
            ["references"] = "int",
        };

    private static readonly IReadOnlyDictionary<string, string> EfTypeMappings =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["string"] = "string",
            ["int"] = "int",
            ["bool"] = "bool",
            ["text"] = "string",
            ["decimal"] = "decimal(18,2)",
            ["datetime"] = "datetime2",
            ["references"] = "int",
        };

    /// <summary>
    /// Parses an array of "name:type" field strings into structured field definitions.
    /// Defaults to "string" when no type is specified.
    /// </summary>
    /// <param name="fieldArgs">The field arguments (e.g., "title:string", "age:int", "author:references").</param>
    /// <returns>A list of parsed field definitions.</returns>
    public static IReadOnlyList<FieldDefinition> Parse(string[] fieldArgs)
    {
        var fields = new List<FieldDefinition>();

        foreach (var arg in fieldArgs)
        {
            var parts = arg.Split(':', 2);
            var name = ToPascalCase(parts[0]);
            var rawType = parts.Length > 1 ? parts[1] : "string";

            if (!TypeMappings.TryGetValue(rawType, out var csharpType))
            {
                ConsoleOutput.Warning($"Unknown field type '{rawType}' for '{name}', defaulting to string.");
                csharpType = "string";
            }

            var isReference = rawType.Equals("references", StringComparison.OrdinalIgnoreCase);
            var fieldName = isReference ? $"{name}Id" : name;
            var referencedModel = isReference ? name : null;

            fields.Add(new FieldDefinition(
                Name: fieldName,
                Type: csharpType,
                RawType: rawType,
                IsReference: isReference,
                ReferencedModel: referencedModel));
        }

        return fields;
    }

    /// <summary>
    /// Gets the EF Core column type for a given field raw type.
    /// </summary>
    /// <param name="rawType">The raw type string from the CLI (e.g., "text", "decimal").</param>
    /// <returns>The EF Core column type string.</returns>
    public static string GetEfColumnType(string rawType)
    {
        return EfTypeMappings.TryGetValue(rawType, out var efType)
            ? efType
            : "nvarchar(max)";
    }

    private static string ToPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        // Handle snake_case
        if (input.Contains('_'))
        {
            var parts = input.Split('_', StringSplitOptions.RemoveEmptyEntries);
            return string.Concat(parts.Select(Capitalize));
        }

        return Capitalize(input);
    }

    private static string Capitalize(string word)
    {
        if (string.IsNullOrEmpty(word))
        {
            return word;
        }

        return char.ToUpperInvariant(word[0]) + word[1..];
    }
}

/// <summary>
/// Represents a parsed field definition for code generation.
/// </summary>
/// <param name="Name">The PascalCase property name.</param>
/// <param name="Type">The C# type name.</param>
/// <param name="RawType">The original type string from the CLI.</param>
/// <param name="IsReference">Whether this field is a foreign key reference.</param>
/// <param name="ReferencedModel">The referenced model name if this is a reference field.</param>
public sealed record FieldDefinition(
    string Name,
    string Type,
    string RawType,
    bool IsReference,
    string? ReferencedModel);
