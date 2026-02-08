using static Volt.Cli.Helpers.NamingConventions;

namespace Volt.Cli.Helpers;

/// <summary>
/// Registers new model types as DbSet properties in AppDbContext.cs.
/// </summary>
public static class DbContextRegistrar
{
    /// <summary>
    /// Adds a DbSet property for the given model to AppDbContext.cs.
    /// Skips if the DbSet already exists or the file cannot be found.
    /// </summary>
    public static void RegisterModel(ProjectContext context, string modelName)
    {
        var layout = context.Layout;
        var dbContextPath = layout.ResolveDbContextPath();

        if (!File.Exists(dbContextPath))
        {
            ConsoleOutput.Warning("AppDbContext.cs not found. Skipping DbSet registration.");
            return;
        }

        var content = File.ReadAllText(dbContextPath);
        var pluralName = Pluralize(modelName);
        var dbSetProperty = $"DbSet<{modelName}>";

        if (content.Contains(dbSetProperty, StringComparison.Ordinal))
        {
            return;
        }

        var modelsUsing = $"using {layout.GetModelNamespace()};";
        if (!content.Contains(modelsUsing, StringComparison.Ordinal))
        {
            content = modelsUsing + Environment.NewLine + content;
        }

        var updatedContent = InsertDbSetProperty(content, modelName, pluralName);

        if (updatedContent is null)
        {
            ConsoleOutput.Warning("Could not determine where to insert DbSet property in AppDbContext.cs.");
            return;
        }

        File.WriteAllText(dbContextPath, updatedContent);
        ConsoleOutput.FileModified("AppDbContext.cs");
    }

    private static string? InsertDbSetProperty(string content, string modelName, string pluralName)
    {
        var dbSetLine = $"    public DbSet<{modelName}> {pluralName} => Set<{modelName}>();";
        var lines = content.Split('\n').ToList();

        var lastDbSetIndex = FindLastDbSetLine(lines);
        if (lastDbSetIndex >= 0)
        {
            lines.Insert(lastDbSetIndex + 1, dbSetLine);
            return string.Join('\n', lines);
        }

        var constructorEndIndex = FindConstructorClosingBrace(lines);
        if (constructorEndIndex >= 0)
        {
            lines.Insert(constructorEndIndex + 1, "");
            lines.Insert(constructorEndIndex + 2, dbSetLine);
            return string.Join('\n', lines);
        }

        return null;
    }

    private static int FindLastDbSetLine(List<string> lines)
    {
        var lastIndex = -1;

        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i].Contains("DbSet<", StringComparison.Ordinal))
            {
                lastIndex = i;
            }
        }

        return lastIndex;
    }

    private static int FindConstructorClosingBrace(List<string> lines)
    {
        var inConstructor = false;
        var braceDepth = 0;

        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];

            if (!inConstructor && line.Contains("public AppDbContext(", StringComparison.Ordinal))
            {
                inConstructor = true;
            }

            if (inConstructor)
            {
                foreach (var ch in line)
                {
                    if (ch == '{') braceDepth++;
                    if (ch == '}') braceDepth--;
                }

                if (braceDepth == 0 && line.TrimStart().StartsWith('}'))
                {
                    return i;
                }
            }
        }

        return -1;
    }
}
