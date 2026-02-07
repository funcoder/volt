using System.Reflection;
using Scriban;
using Scriban.Runtime;

namespace Volt.Cli.Helpers;

/// <summary>
/// Scriban template rendering engine that loads templates from embedded resources.
/// Templates are stored under the Templates/ directory and compiled on first access.
/// </summary>
public static class TemplateEngine
{
    private static readonly Assembly ResourceAssembly = typeof(TemplateEngine).Assembly;
    private const string ResourcePrefix = "Volt.Cli.Templates.";

    /// <summary>
    /// Loads a raw template string from the embedded resources.
    /// </summary>
    /// <param name="templateName">The template name without path prefix (e.g., "Model", "Controller").</param>
    /// <returns>The raw template string.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the template resource is not found.</exception>
    public static string LoadTemplate(string templateName)
    {
        var resourceName = $"{ResourcePrefix}{templateName}.scriban";
        using var stream = ResourceAssembly.GetManifestResourceStream(resourceName);

        if (stream is null)
        {
            throw new InvalidOperationException(
                $"Template '{templateName}' not found. Expected embedded resource: {resourceName}");
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Renders a Scriban template with the specified model data.
    /// </summary>
    /// <param name="templateName">The template name without path prefix (e.g., "Model", "Controller").</param>
    /// <param name="model">An anonymous object or dictionary containing template variables.</param>
    /// <returns>The rendered template output.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the template has parse errors.</exception>
    public static string Render(string templateName, object model)
    {
        var source = LoadTemplate(templateName);
        var template = Template.Parse(source);

        if (template.HasErrors)
        {
            var errors = string.Join("; ", template.Messages);
            throw new InvalidOperationException(
                $"Template '{templateName}' has parse errors: {errors}");
        }

        var scriptObject = new ScriptObject();
        scriptObject.Import(model);

        var context = new TemplateContext();
        context.PushGlobal(scriptObject);

        return template.Render(context);
    }

    /// <summary>
    /// Renders a template and writes the output to a file, creating directories as needed.
    /// If the file already exists, it is skipped and a warning is shown.
    /// </summary>
    /// <param name="templateName">The template name.</param>
    /// <param name="model">The template model data.</param>
    /// <param name="outputPath">The full path to write the rendered output.</param>
    /// <returns>True if the file was created; false if it was skipped.</returns>
    public static bool RenderToFile(string templateName, object model, string outputPath)
    {
        if (File.Exists(outputPath))
        {
            var relativePath = Path.GetFileName(outputPath);
            ConsoleOutput.FileSkipped(relativePath);
            return false;
        }

        var directory = Path.GetDirectoryName(outputPath);

        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var content = Render(templateName, model);
        File.WriteAllText(outputPath, content);

        return true;
    }
}
