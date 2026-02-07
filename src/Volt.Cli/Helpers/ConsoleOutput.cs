namespace Volt.Cli.Helpers;

/// <summary>
/// Provides styled console output helpers for the Volt CLI.
/// All user-facing output should go through this class to ensure
/// consistent formatting and color usage.
/// </summary>
public static class ConsoleOutput
{
    private const string VoltBanner = """

        __     __    _ _
        \ \   / /__ | | |_
         \ \ / / _ \| | __|
          \ V / (_) | | |_
           \_/ \___/|_|\__|

        Rails-like framework for .NET

        """;

    /// <summary>
    /// Displays the Volt ASCII art banner in cyan.
    /// </summary>
    public static void Banner()
    {
        WriteColored(VoltBanner, ConsoleColor.Cyan);
    }

    /// <summary>
    /// Writes a success message in green with a checkmark prefix.
    /// </summary>
    /// <param name="message">The success message to display.</param>
    public static void Success(string message)
    {
        WriteColored($"  [OK] {message}", ConsoleColor.Green);
    }

    /// <summary>
    /// Writes an error message in red with an X prefix.
    /// </summary>
    /// <param name="message">The error message to display.</param>
    public static void Error(string message)
    {
        WriteColored($"  [ERROR] {message}", ConsoleColor.Red);
    }

    /// <summary>
    /// Writes a warning message in yellow with an exclamation prefix.
    /// </summary>
    /// <param name="message">The warning message to display.</param>
    public static void Warning(string message)
    {
        WriteColored($"  [WARN] {message}", ConsoleColor.Yellow);
    }

    /// <summary>
    /// Writes an informational message in blue with an info prefix.
    /// </summary>
    /// <param name="message">The informational message to display.</param>
    public static void Info(string message)
    {
        WriteColored($"  [INFO] {message}", ConsoleColor.Blue);
    }

    /// <summary>
    /// Writes a plain message without any prefix or color.
    /// </summary>
    /// <param name="message">The message to display.</param>
    public static void Plain(string message)
    {
        Console.WriteLine(message);
    }

    /// <summary>
    /// Writes a blank line to the console.
    /// </summary>
    public static void BlankLine()
    {
        Console.WriteLine();
    }

    /// <summary>
    /// Writes a line indicating a file was created.
    /// </summary>
    /// <param name="relativePath">The relative file path that was created.</param>
    public static void FileCreated(string relativePath)
    {
        WriteColored($"  create  {relativePath}", ConsoleColor.Green);
    }

    /// <summary>
    /// Writes a line indicating a file was deleted.
    /// </summary>
    /// <param name="relativePath">The relative file path that was deleted.</param>
    public static void FileDeleted(string relativePath)
    {
        WriteColored($"  remove  {relativePath}", ConsoleColor.Red);
    }

    /// <summary>
    /// Writes a line indicating a file was skipped because it already exists.
    /// </summary>
    /// <param name="relativePath">The relative file path that was skipped.</param>
    public static void FileSkipped(string relativePath)
    {
        WriteColored($"  skip    {relativePath}", ConsoleColor.Yellow);
    }

    /// <summary>
    /// Displays tabular data with headers and aligned columns.
    /// </summary>
    /// <param name="headers">The column header names.</param>
    /// <param name="rows">The rows of data, each row being an array of column values.</param>
    public static void Table(string[] headers, IReadOnlyList<string[]> rows)
    {
        if (headers.Length == 0)
        {
            return;
        }

        var columnWidths = CalculateColumnWidths(headers, rows);
        var separator = BuildSeparator(columnWidths);

        WriteSeparator(separator);
        WriteTableRow(headers, columnWidths, ConsoleColor.Cyan);
        WriteSeparator(separator);

        foreach (var row in rows)
        {
            WriteTableRow(row, columnWidths, foreground: null);
        }

        WriteSeparator(separator);
    }

    private static int[] CalculateColumnWidths(string[] headers, IReadOnlyList<string[]> rows)
    {
        var widths = new int[headers.Length];

        for (var i = 0; i < headers.Length; i++)
        {
            widths[i] = headers[i].Length;
        }

        foreach (var row in rows)
        {
            for (var i = 0; i < Math.Min(row.Length, headers.Length); i++)
            {
                widths[i] = Math.Max(widths[i], row[i].Length);
            }
        }

        return widths;
    }

    private static string BuildSeparator(int[] columnWidths)
    {
        var parts = new string[columnWidths.Length];

        for (var i = 0; i < columnWidths.Length; i++)
        {
            parts[i] = new string('-', columnWidths[i] + 2);
        }

        return $"+-{string.Join("-+-", parts)}-+";
    }

    private static void WriteSeparator(string separator)
    {
        Console.WriteLine($"  {separator}");
    }

    private static void WriteTableRow(string[] values, int[] widths, ConsoleColor? foreground)
    {
        var cells = new string[widths.Length];

        for (var i = 0; i < widths.Length; i++)
        {
            var value = i < values.Length ? values[i] : string.Empty;
            cells[i] = value.PadRight(widths[i]);
        }

        var line = $"  | {string.Join(" | ", cells)} |";

        if (foreground.HasValue)
        {
            WriteColored(line, foreground.Value);
        }
        else
        {
            Console.WriteLine(line);
        }
    }

    private static void WriteColored(string message, ConsoleColor color)
    {
        var previous = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ForegroundColor = previous;
    }
}
