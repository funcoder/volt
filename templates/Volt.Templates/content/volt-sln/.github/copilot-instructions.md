This is a Volt Framework multi-project solution (.NET, Rails-like conventions).

Multi-project structure:
- src/VoltApp.Models/ - Entity classes with init-only properties and soft-delete
- src/VoltApp.Data/ - AppDbContext (inherits VoltDbContext), migrations, seeds
- src/VoltApp.Services/ - Jobs, mailers, channels
- src/VoltApp.Web/ - Controllers, Razor views (Tailwind CSS via CDN, HTMX), static files
- tests/VoltApp.Tests/ - Unit and integration tests

Dependency flow: Models <- Data <- Services <- Web

CLI commands:
- volt server - Start dev server with hot reload
- volt generate scaffold <Name> <fields...> - Full CRUD (model, controller, views, migration, tests)
- volt generate model <Name> <fields...> - Model + migration
- volt generate controller <Name> - Empty controller
- volt generate ai-context - Regenerate AI context files
- volt db migrate - Apply migrations
- volt db rollback - Rollback last migration

Field types: string, text, int, bool, decimal, datetime, references (FK), image (upload), file (upload)

Naming conventions:
- Models: PascalCase singular (Post, BlogPost)
- Controllers: PascalCase plural + Controller (PostsController)
- Tables: snake_case plural (posts, blog_posts)
- Columns: snake_case (created_at)
- Routes: snake_case plural (/posts)

Key patterns:
- Immutable models: init setters, use `with` expressions for copies
- Soft delete: DeletedAt column, auto-filtered queries
- Flash messages: Flash.Success("msg") in controllers
- PermittedParams: string[] whitelist of form fields
- HTMX: hx-boost="true" for SPA-like navigation
- Tailwind CSS via CDN with amber brand color
- File uploads via IStorageService and VoltAttachment

ResourceController actions: Index (GET /resources), New (GET /resources/new),
Create (POST /resources), Show (GET /resources/:id), Edit (GET /resources/:id/edit),
Update (PUT /resources/:id), Destroy (DELETE /resources/:id)

When generating code, follow these conventions exactly. Use init-only properties.
Do not mutate objects. Use with expressions instead.
