using System.Text;

namespace Volt.Web.Middleware;

/// <summary>
/// Renders the HTML page shown when pending database migrations are detected.
/// </summary>
internal static class PendingMigrationsPage
{
    public static string Render(IReadOnlyList<string> pendingMigrations)
    {
        var migrationRows = new StringBuilder();

        foreach (var migration in pendingMigrations)
        {
            var name = FormatMigrationName(migration);
            migrationRows.AppendLine(
                $"            <tr><td class=\"id\">{migration}</td><td>{name}</td></tr>");
        }

        return $$"""
        <!DOCTYPE html>
        <html lang="en">
        <head>
            <meta charset="utf-8" />
            <meta name="viewport" content="width=device-width, initial-scale=1" />
            <title>Pending Migrations — Volt</title>
            <style>
                * { margin: 0; padding: 0; box-sizing: border-box; }
                body {
                    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
                    background: #0f172a;
                    color: #e2e8f0;
                    min-height: 100vh;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                }
                .container {
                    max-width: 640px;
                    width: 100%;
                    padding: 2rem;
                }
                .card {
                    background: #1e293b;
                    border: 1px solid #334155;
                    border-radius: 12px;
                    padding: 2.5rem;
                    box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.5);
                }
                .icon {
                    width: 48px;
                    height: 48px;
                    background: #f59e0b;
                    border-radius: 12px;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    margin-bottom: 1.5rem;
                    font-size: 24px;
                }
                h1 {
                    font-size: 1.5rem;
                    font-weight: 700;
                    margin-bottom: 0.5rem;
                    color: #f8fafc;
                }
                .subtitle {
                    color: #94a3b8;
                    margin-bottom: 2rem;
                    line-height: 1.6;
                }
                table {
                    width: 100%;
                    border-collapse: collapse;
                    margin-bottom: 2rem;
                    font-size: 0.875rem;
                }
                th {
                    text-align: left;
                    padding: 0.625rem 0.75rem;
                    background: #0f172a;
                    color: #94a3b8;
                    font-weight: 600;
                    text-transform: uppercase;
                    font-size: 0.75rem;
                    letter-spacing: 0.05em;
                }
                td {
                    padding: 0.625rem 0.75rem;
                    border-top: 1px solid #334155;
                    color: #e2e8f0;
                }
                td.id {
                    font-family: 'SF Mono', SFMono-Regular, Consolas, monospace;
                    font-size: 0.8rem;
                    color: #94a3b8;
                }
                .command-section {
                    margin-bottom: 1.5rem;
                }
                .command-label {
                    font-size: 0.75rem;
                    font-weight: 600;
                    color: #94a3b8;
                    text-transform: uppercase;
                    letter-spacing: 0.05em;
                    margin-bottom: 0.5rem;
                }
                .command {
                    background: #0f172a;
                    border: 1px solid #334155;
                    border-radius: 8px;
                    padding: 0.875rem 1rem;
                    font-family: 'SF Mono', SFMono-Regular, Consolas, monospace;
                    font-size: 0.875rem;
                    color: #f59e0b;
                    user-select: all;
                }
                .badge {
                    display: inline-block;
                    background: #7c2d12;
                    color: #fb923c;
                    font-size: 0.75rem;
                    font-weight: 600;
                    padding: 0.125rem 0.5rem;
                    border-radius: 9999px;
                    margin-left: 0.5rem;
                    vertical-align: middle;
                }
                .footer {
                    text-align: center;
                    margin-top: 1.5rem;
                    color: #475569;
                    font-size: 0.75rem;
                }
                .footer span { color: #f59e0b; }
            </style>
        </head>
        <body>
            <div class="container">
                <div class="card">
                    <div class="icon">&#9888;</div>
                    <h1>
                        Pending Migrations
                        <span class="badge">{{pendingMigrations.Count}}</span>
                    </h1>
                    <p class="subtitle">
                        Your database is behind the current schema. Apply the pending
                        migrations below to get up and running.
                    </p>
                    <table>
                        <thead>
                            <tr>
                                <th>Migration ID</th>
                                <th>Name</th>
                            </tr>
                        </thead>
                        <tbody>
        {{migrationRows}}                </tbody>
                    </table>
                    <div class="command-section">
                        <div class="command-label">Run in your terminal</div>
                        <div class="command">volt db:migrate</div>
                    </div>
                </div>
                <div class="footer">
                    <span>&#9889;</span> Volt Framework — This page only appears in development
                </div>
            </div>
        </body>
        </html>
        """;
    }

    private static string FormatMigrationName(string migrationId)
    {
        var underscoreIndex = migrationId.IndexOf('_');
        if (underscoreIndex < 0) return migrationId;

        var name = migrationId[(underscoreIndex + 1)..];

        var result = new StringBuilder();
        for (var i = 0; i < name.Length; i++)
        {
            if (char.IsUpper(name[i]) && i > 0)
            {
                result.Append(' ');
            }
            result.Append(name[i]);
        }

        return result.ToString();
    }
}
