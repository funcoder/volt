# Volt

**Rails-like opinionated framework for .NET 10.**

Volt brings convention-over-configuration productivity to .NET. It's a curated collection of NuGet packages unified by a CLI tool and opinionated project templates - a layer on ASP.NET Core, not a replacement.

Build full-stack web apps or APIs with zero boilerplate. Volt's CLI generates multi-project solutions with clean separation of concerns, and source generators wire everything together at compile time.

## Quick Start

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (10.0.100 or later)

### Install the CLI

```bash
dotnet tool install -g VoltFramework.Cli
```

### Create a New Project

```bash
# Multi-project solution (default - Models, Data, Services, Web)
volt new MyApp

# Single-project layout (all code in one project)
volt new MyApp --simple

# API-only project (JSON, JWT auth, Swagger)
volt new MyApp --api

# Specify a database provider (default: sqlite)
volt new MyApp --database sqlite
volt new MyApp --database postgres
volt new MyApp --database sqlserver
```

### Start the Dev Server

```bash
cd MyApp
volt server
```

```
  ⚡ Volt v0.3.0 - Development Server
  ────────────────────────────────────
  → http://localhost:5000
  → Database: SQLite (myapp_development.db)
  → Hot Reload: enabled
  → Press Ctrl+C to stop
```

### Generate Code

```bash
# Generate a model with typed fields
volt generate model Article title:string body:text published:bool

# Generate a full CRUD scaffold (model + migration + controller + views + tests)
volt generate scaffold Post title:string body:text author:references

# Generate individual components
volt generate controller Articles
volt generate migration AddEmailToUsers email:string
volt generate job ProcessImage
volt generate mailer UserMailer welcome_email
volt generate channel ChatChannel
```

### Manage the Database

```bash
volt db migrate             # Run pending migrations
volt db rollback            # Rollback last migration
volt db rollback --steps 3  # Rollback 3 migrations
volt db seed                # Run Seeds/SeedData.cs
volt db reset               # Drop, create, migrate, seed
volt db status              # Show migration status
```

### Docker Database Management

Spin up a PostgreSQL or SQL Server container with a single command. Volt handles the Docker lifecycle and automatically switches your project's database provider:

```bash
# Start a PostgreSQL container (switches from SQLite automatically)
volt db docker up postgres

# Start a SQL Server container
volt db docker up sqlserver

# Auto-detect provider from project and start container
volt db docker up

# Container lifecycle
volt db docker status        # Show container status
volt db docker logs          # Tail container logs
volt db docker down          # Stop and remove the container
```

When switching providers, Volt automatically updates your `.csproj` package references, `AppDbContext.cs` configuration, and runs `dotnet restore`. Requires [Docker Desktop](https://www.docker.com/products/docker-desktop) or Docker Engine.

---

## Project Structure

By default, `volt new` creates a multi-project solution with clean separation of concerns:

```
MyApp/
├── src/
│   ├── MyApp.Models/           # Entity classes
│   ├── MyApp.Data/             # DbContext, migrations, seeds
│   ├── MyApp.Services/         # Jobs, mailers, channels
│   └── MyApp.Web/              # Controllers, views, static files, Program.cs
├── tests/
│   └── MyApp.Tests/            # Unit and integration tests
├── MyApp.sln
├── CLAUDE.md                   # AI context (auto-generated)
└── .cursorrules
```

**Dependency flow:** `Models ← Data ← Services ← Web`

Each generator places code in the correct project automatically:

| Generator | Project | Path |
|-----------|---------|------|
| model | MyApp.Models | `ModelName.cs` |
| controller | MyApp.Web | `Controllers/ModelsController.cs` |
| views | MyApp.Web | `Views/Models/*.cshtml` |
| migration | MyApp.Data | `Migrations/*.cs` |
| job | MyApp.Services | `Jobs/JobName.cs` |
| mailer | MyApp.Services | `Mailers/MailerName.cs` |
| channel | MyApp.Services | `Channels/ChannelName.cs` |
| test | MyApp.Tests | `Models/` or `Controllers/` |

Use `volt new MyApp --simple` for a single-project layout with everything in one directory.

Source generators discover classes by folder convention and wire everything at compile time - no runtime reflection, fully AOT-compatible, and inspectable in `obj/Generated/`.

---

## Core Concepts

### Models

Models inherit from `Model<T>` and get automatic timestamps, soft deletes, and database registration:

```csharp
// Models/Article.cs
public class Article : Model<Article>
{
    public string Title { get; init; } = "";
    public string Body { get; init; } = "";
    public bool Published { get; init; }

    // Associations
    public User Author { get; init; } = null!;
    public IReadOnlyList<Comment> Comments { get; init; } = [];

    // Validations
    public static void ConfigureValidations(ValidationBuilder<Article> v)
    {
        v.Property(a => a.Title).Required().MaxLength(255);
        v.Property(a => a.Body).Required();
    }

    // Scopes
    public static IQueryable<Article> Published(IQueryable<Article> query)
        => query.Where(a => a.Published);

    public static IQueryable<Article> Recent(IQueryable<Article> query)
        => query.OrderByDescending(a => a.CreatedAt);
}
```

Every model automatically gets:
- `Id` (int primary key)
- `CreatedAt` / `UpdatedAt` (auto-managed timestamps)
- `DeletedAt` (soft-delete support)
- Registered as a `DbSet<T>` on the DbContext

### Controllers

`ResourceController<T>` provides full CRUD with strong parameters and flash messages:

```csharp
// Controllers/ArticlesController.cs
public class ArticlesController : ResourceController<Article>
{
    public ArticlesController(VoltDbContext db) : base(db) { }

    // Whitelist allowed properties
    public override string[] PermittedParams => ["Title", "Body", "Published"];

    // Override only what you need - defaults handle standard CRUD:
    // GET    /articles         → Index()
    // GET    /articles/{id}    → Show(id)
    // GET    /articles/new     → New()
    // POST   /articles         → Create()
    // GET    /articles/{id}/edit → Edit(id)
    // PUT    /articles/{id}    → Update(id)
    // DELETE /articles/{id}    → Destroy(id)

    // Add custom actions
    [HttpPost("{id}/publish")]
    public async Task<IActionResult> Publish(int id)
    {
        var article = await Find(id);
        if (article is null) return NotFound();

        var published = article with { Published = true };
        Db.Set<Article>().Update(published);
        await Db.SaveChangesAsync();
        return RedirectTo(published, flash: "Article published.");
    }
}
```

For APIs, use `ApiResourceController<T>` which returns JSON with automatic pagination:

```csharp
public class ArticlesController : ApiResourceController<Article>
{
    public ArticlesController(VoltDbContext db) : base(db) { }
    public override string[] PermittedParams => ["Title", "Body", "Published"];

    // GET    /articles     → 200 JSON array (paginated)
    // GET    /articles/1   → 200 JSON object
    // POST   /articles     → 201 with Location header
    // PUT    /articles/1   → 200 JSON object
    // DELETE /articles/1   → 204 No Content
}
```

### Background Jobs

Define jobs with typed payloads. Jobs are auto-discovered and registered:

```csharp
// Jobs/ProcessImageJob.cs
public class ProcessImageJob : VoltJob<ProcessImagePayload>
{
    private readonly IStorageService _storage;

    public ProcessImageJob(IStorageService storage) => _storage = storage;

    public override async Task Execute(ProcessImagePayload payload)
    {
        var image = await _storage.Retrieve(payload.ImageId);
        // Process the image...
    }
}

public record ProcessImagePayload(string ImageId);

// Enqueue from anywhere via IJobQueue
await jobQueue.Enqueue<ProcessImageJob>(new ProcessImagePayload("img-123"));
```

### Mailers

Send emails with Razor templates and a fluent API:

```csharp
// Mailers/UserMailer.cs
public class UserMailer : VoltMailer
{
    public UserMailer(IFluentEmail email, IOptions<VoltMailerOptions> options)
        : base(email, options) { }

    public async Task WelcomeEmail(User user)
    {
        To(user.Email);
        Subject("Welcome to our app!");
        // Uses Views/Mailers/UserMailer/WelcomeEmail.cshtml
        await Send(new { User = user });
    }
}
```

Mailer previews are available at `/volt/mailers/` in development.

### Real-Time Channels

SignalR-based channels with convention routing:

```csharp
// Channels/ChatChannel.cs → maps to /volt/channels/chat
public class ChatChannel : VoltChannel
{
    public override async Task OnSubscribed(string connectionId)
    {
        await Groups.AddToGroupAsync(connectionId, "chat");
    }
}

// Broadcast from anywhere via IChannelBroadcaster
await broadcaster.Broadcast<ChatChannel>("NewMessage", new { text = "Hello!" });
```

### File Storage

Declarative file attachments on models with pluggable backends:

```csharp
// Models/User.cs
public class User : Model<User>
{
    public string Name { get; init; } = "";

    [HasOneAttached]
    public VoltAttachment Avatar { get; init; } = null!;

    [HasManyAttached]
    public IReadOnlyList<VoltAttachment> Documents { get; init; } = [];
}
```

Configure storage backends:

```csharp
builder.Services.AddVoltStorage(storage =>
{
    storage.Service("local", s => s.Disk("./storage"));
    storage.Service("s3", s => s.S3("my-bucket", "us-east-1"));
    storage.Default("local");
});
```

### Authentication

Opinionated ASP.NET Core Identity setup:

```csharp
builder.Services.AddVoltAuth<AppDbContext>(auth =>
{
    auth.RequireConfirmedEmail = false;
    auth.PasswordMinLength = 8;
    auth.LockoutMaxAttempts = 5;
    auth.SessionTimeout = TimeSpan.FromDays(14);
});

app.UseVoltAuth();
```

### Testing

FactoryBot-inspired test factories and a `WebApplicationFactory` base class:

```csharp
public class ArticleTests : VoltTestBase<Program>
{
    [Fact]
    public async Task CreateArticle_WithValidData_Succeeds()
    {
        // Define factories
        Factory.Define(() => new Article
        {
            Title = "Test Article",
            Body = "Test body content",
            Published = false
        });

        // Build in memory (no DB)
        var article = Factory.Build<Article>();

        // Or create and persist
        var persisted = await Factory.Create<Article>(Db);

        // Build a list
        var articles = Factory.BuildList<Article>(5, (a, i) =>
        {
            // customize with index
        });
    }
}
```

Custom assertions:

```csharp
article.ShouldBeValid();
response.ShouldRedirectTo("/articles/1");
response.ShouldHaveStatusCode(200);
```

---

## Database Configuration

SQLite is the default for development - zero setup, just works. Switch to PostgreSQL or SQL Server for production through configuration alone:

```json
// appsettings.json (development - SQLite)
{
  "Volt": {
    "Database": {
      "Provider": "sqlite"
    }
  }
}

// appsettings.Production.json (PostgreSQL)
{
  "Volt": {
    "Database": {
      "Provider": "postgres",
      "ConnectionString": "Host=localhost;Database=myapp;Username=postgres;Password=secret"
    }
  }
}
```

All three EF Core providers (SQLite, PostgreSQL, SQL Server) are included by default, so switching is instant with no package changes.

### Database Conventions

Configure EF Core conventions in `Config/Database.cs`:

```csharp
public static void Configure(VoltDbOptions db)
{
    db.DefaultProvider = DbProvider.Sqlite;
    db.Timestamps();      // Auto-manage CreatedAt, UpdatedAt
    db.SoftDeletes();     // Auto-manage DeletedAt
    db.Pluralize();       // Table names: User → users
    db.SnakeCase();       // Column names: FirstName → first_name
}
```

---

## CLI Reference

| Command | Description |
|---------|-------------|
| `volt new <name>` | Create a new Volt project (multi-project solution) |
| `volt new <name> --simple` | Create a single-project Volt app |
| `volt server` | Start the development server (wraps `dotnet watch run`) |
| `volt console` | Open a C# REPL with app context |
| `volt routes` | List all registered routes |
| `volt generate model <Name> [fields...]` | Generate a model and migration |
| `volt generate controller <Name>` | Generate a controller |
| `volt generate scaffold <Name> [fields...]` | Generate model + migration + controller + views + tests |
| `volt generate migration <Name> [fields...]` | Generate a database migration |
| `volt generate job <Name>` | Generate a background job |
| `volt generate mailer <Name> [methods...]` | Generate a mailer with email methods |
| `volt generate channel <Name>` | Generate a real-time channel |
| `volt db migrate` | Run pending EF Core migrations |
| `volt db rollback [--steps N]` | Rollback migrations |
| `volt db seed` | Run the database seeder |
| `volt db reset` | Drop, recreate, migrate, and seed |
| `volt db status` | Show migration status |
| `volt db docker up [provider]` | Start a Docker database container (postgres or sqlserver) |
| `volt db docker down` | Stop and remove the Docker database container |
| `volt db docker status` | Show Docker container status |
| `volt db docker logs` | Tail Docker container logs |
| `volt destroy <type> <Name>` | Reverse a generate command |

### Field Type Syntax

When generating models or scaffolds, fields use `name:type` syntax:

| Type | C# Type | Database Column |
|------|---------|----------------|
| `string` | `string` | `nvarchar` / `text` |
| `text` | `string` | `text` (long content) |
| `int` | `int` | `integer` |
| `long` | `long` | `bigint` |
| `decimal` | `decimal` | `decimal(18,2)` |
| `float` | `float` | `real` |
| `double` | `double` | `double` |
| `bool` | `bool` | `boolean` |
| `datetime` | `DateTime` | `timestamp` |
| `guid` | `Guid` | `uniqueidentifier` |
| `references` | `int` (FK) | `integer` + navigation property |
| `image` | `int?` (FK) | Image upload with preview |
| `file` | `int?` (FK) | File upload with download |

---

## NuGet Packages

Volt is modular - use only what you need, or install the meta-package for everything:

| Package | Description |
|---------|-------------|
| `VoltFramework` | Meta-package (includes all packages below) |
| `VoltFramework.Core` | Conventions, base classes, attributes, validation |
| `VoltFramework.Core.Generators` | Source generators for auto-wiring (compile-time) |
| `VoltFramework.Data` | EF Core conventions, `VoltDbContext`, query extensions |
| `VoltFramework.Web` | `ResourceController<T>`, routing, HTMX helpers, tag helpers |
| `VoltFramework.Auth` | ASP.NET Core Identity setup with opinionated defaults |
| `VoltFramework.Jobs` | Background job abstraction over Coravel |
| `VoltFramework.Mailer` | Email system with Razor templates and FluentEmail |
| `VoltFramework.Storage` | File attachments with local disk and cloud backends |
| `VoltFramework.RealTime` | SignalR channel conventions and broadcasting |
| `VoltFramework.Testing` | Test factories, assertions, `WebApplicationFactory` base |
| `VoltFramework.Cli` | CLI tool (`dotnet tool`) for project management |
| `VoltFramework.Templates` | `dotnet new` project templates |

---

## Building from Source

```bash
git clone git@github.com:funcoder/volt.git
cd volt
dotnet restore
dotnet build
```

### Running Tests

```bash
dotnet test
```

### Installing the CLI Locally

```bash
dotnet pack -c Release -o artifacts
dotnet tool install -g VoltFramework.Cli --add-source artifacts
```

### Installing Templates Locally

```bash
dotnet pack templates/Volt.Templates/Volt.Templates.csproj -c Release -o artifacts
dotnet new install artifacts/VoltFramework.Templates.0.3.0.nupkg
```

---

## Architecture

Volt is designed as a **layer on ASP.NET Core**, not a replacement. Every piece can be understood and debugged using standard .NET tools.

### Source Generators over Runtime Reflection

All convention-based wiring happens at compile time via Roslyn incremental source generators:

- **ModelDiscoveryGenerator** - Discovers `Model<T>` subclasses in `Models/` and registers them as `DbSet<T>` properties
- **ServiceRegistrationGenerator** - Discovers classes in `Services/` and auto-registers them in DI
- **RouteRegistrationGenerator** - Discovers controllers and generates RESTful route mappings

Generated code is fully inspectable in `obj/Generated/` - no hidden magic.

### Immutability by Default

Models use `init`-only properties and `with` expressions throughout:

```csharp
// Create
var article = new Article { Title = "Hello", Body = "World" };

// Update (new object, never mutate)
var updated = article with { Title = "New Title" };
```

### Technology Stack

| Concern | Technology |
|---------|-----------|
| Web framework | ASP.NET Core 10 |
| ORM | Entity Framework Core 10 |
| Auth | ASP.NET Core Identity |
| Real-time | SignalR |
| Background jobs | Coravel |
| Email | FluentEmail + MailKit |
| File storage | Custom abstraction (local disk, S3) |
| Frontend | Blazor SSR + HTMX |
| CLI | System.CommandLine |
| Code generation | Scriban templates |
| Testing | xUnit + WebApplicationFactory |

---

## Roadmap

### Phase 1 - Foundation (current)
- [x] Project templates (`dotnet new volt`, `dotnet new volt-sln`, `dotnet new volt-api`)
- [x] Multi-project solution layout (Models, Data, Services, Web) as default
- [x] CLI tool with `new`, `generate`, `server`, `console`, `db *`, `routes`, `destroy`
- [x] Source generators for model discovery, service registration, and route generation
- [x] EF Core conventions (timestamps, soft deletes, snake_case, pluralization)
- [x] `ResourceController<T>` and `ApiResourceController<T>` with CRUD defaults
- [x] Full scaffold generation (model + migration + controller + views + tests)
- [x] Background jobs, mailer, storage, real-time channels
- [x] File/image attachments with upload support
- [x] Test factories and assertion helpers
- [x] AI context generation (CLAUDE.md, .cursorrules, copilot-instructions.md)

### Phase 2 - Productivity
- [ ] Validation framework integration
- [ ] Enhanced REPL with full app context
- [ ] HTMX tag helpers and Blazor SSR components
- [ ] Caching helpers (HybridCache, fragment caching)

### Phase 3 - Full Stack
- [ ] `volt generate authentication` (login, register, password reset, 2FA)
- [ ] File storage variants (image resizing)
- [ ] i18n / localization conventions
- [ ] Plugin system (`IVoltPlugin`)
- [ ] Rich text editor component

### Phase 4 - Deployment
- [ ] `volt deploy setup azure` (Bicep templates, Container Apps)
- [ ] `volt deploy setup server` (Docker + SSH, Caddy reverse proxy)
- [ ] Zero-downtime deployment strategies
- [ ] Production monitoring and error pages

---

## Philosophy

1. **Convention over configuration** - Sensible defaults for everything. Override only when you need to.
2. **Layer, not framework** - Built on ASP.NET Core, not replacing it. Standard .NET debugging and tooling works.
3. **Compile-time wiring** - Source generators, not runtime reflection. AOT-compatible and inspectable.
4. **Batteries included** - ORM, auth, jobs, email, storage, real-time, testing - all pre-wired.
5. **Immutable by design** - `init`-only properties and `with` expressions throughout.

---

## License

MIT
