# Volt Framework - Claude Instructions

## Code Style

### File Size Limit
All classes/files must be no more than ~300 lines of code. If a file exceeds this limit, refactor by extracting cohesive pieces into separate files. Prefer many small, focused files over few large files.

### Immutability
Use `init`-only properties and `with` expressions throughout. Never mutate existing objects.

### Naming Conventions
- Models: PascalCase (e.g., `Article`, `User`)
- Controllers: PascalCase with `Controller` suffix (e.g., `ArticlesController`)
- Table names: snake_case, pluralized (e.g., `articles`, `blog_posts`)
- Column names: snake_case (e.g., `created_at`, `first_name`)

## Project Structure

- `src/` - All library and CLI source projects
- `templates/` - `dotnet new` project templates
- `tests/` - Test projects
- `artifacts/` - Local NuGet packages (not committed)

## Build

```bash
dotnet build Volt.sln          # Build everything
dotnet test                     # Run tests
dotnet pack -c Release -o artifacts  # Create NuGet packages
```

## Local Development

The CLI tool (`volt`) is distributed as a `dotnet tool`. For local testing:

```bash
dotnet pack src/Volt.Cli/Volt.Cli.csproj -c Release -o artifacts
dotnet tool install -g Volt.Cli --add-source artifacts --version 0.1.0
```

Templates must be packed and installed separately:

```bash
dotnet pack templates/Volt.Templates/Volt.Templates.csproj -c Release -o artifacts
dotnet new install artifacts/Volt.Templates.0.1.0.nupkg
```

A `VoltLocal` NuGet source pointing to `artifacts/` must be configured in the global NuGet.Config for generated projects to resolve Volt packages.
