# VoltApp

A web application built with the Volt Framework (.NET, Rails-like conventions).

## Project Structure

This is a multi-project solution with clean separation of concerns:

```
src/
├── VoltApp.Models/         Entity classes (namespace: VoltApp.Models)
├── VoltApp.Data/           DbContext, migrations, seeds (namespace: VoltApp.Data)
├── VoltApp.Services/       Jobs, mailers, channels (namespace: VoltApp.Services)
└── VoltApp.Web/            Controllers, views, static files (namespace: VoltApp.Web)
tests/
└── VoltApp.Tests/          Unit and integration tests
```

### Dependency Flow
```
VoltApp.Models  <-  VoltApp.Data  <-  VoltApp.Services  <-  VoltApp.Web
```

### Where Code Goes
| Generator | Project | Path |
|-----------|---------|------|
| model | VoltApp.Models | `ModelName.cs` (root) |
| controller | VoltApp.Web | `Controllers/ModelsController.cs` |
| views | VoltApp.Web | `Views/Models/*.cshtml` |
| migration | VoltApp.Data | `Migrations/*.cs` |
| seed | VoltApp.Data | `Seeds/*.cs` |
| job | VoltApp.Services | `Jobs/JobName.cs` |
| mailer | VoltApp.Services | `Mailers/MailerName.cs` |
| channel | VoltApp.Services | `Channels/ChannelName.cs` |
| test | VoltApp.Tests | `Models/` or `Controllers/` |

## CLI Commands

```bash
volt server                    # Start dev server (hot reload)
volt generate scaffold Post title:string body:text published:bool
volt generate model Comment body:text post:references
volt generate controller Api::Health
volt generate job SendEmail
volt generate mailer Welcome
volt generate channel Chat
volt generate migration AddSlugToPosts
volt generate ai-context       # Regenerate AI context files
volt db migrate                # Apply pending migrations
volt db rollback               # Rollback last migration
volt db seed                   # Run database seeds
```

## Field Types

| CLI Type     | C# Type    | Notes                        |
|-------------|------------|------------------------------|
| string      | string     | Default type                 |
| text        | string     | Textarea in forms            |
| int         | int        |                              |
| bool        | bool       | Checkbox in forms            |
| decimal     | decimal    |                              |
| datetime    | DateTime   |                              |
| references  | int        | Foreign key (e.g. post:references) |
| image       | int?       | Image upload with preview    |
| file        | int?       | File upload with download    |

## Naming Conventions

- Models: PascalCase singular (`Post`, `BlogPost`)
- Controllers: PascalCase plural + Controller (`PostsController`)
- Table names: snake_case plural (`posts`, `blog_posts`)
- Column names: snake_case (`created_at`, `first_name`)
- Routes: snake_case plural (`/posts`, `/blog_posts`)

## Key Patterns

- **Immutable models**: Properties use `init` setters; use `with` expressions
- **Soft delete**: Models have `DeletedAt`; queries auto-filter deleted records
- **Flash messages**: `Flash.Success("Created!")` in controllers
- **PermittedParams**: Whitelist of allowed form fields per controller
- **HTMX**: `hx-boost="true"` on body for SPA-like navigation
- **Tailwind CSS**: Loaded via CDN; brand color is amber

## Build & Run

```bash
dotnet build          # Build the solution
dotnet test           # Run tests
volt server           # Start with hot reload
```
