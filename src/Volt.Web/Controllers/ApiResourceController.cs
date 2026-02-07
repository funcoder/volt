using System.Linq.Expressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Volt.Core;
using Volt.Data;

namespace Volt.Web.Controllers;

/// <summary>
/// Base API controller providing conventional CRUD actions that return JSON responses.
/// Override <see cref="PermittedParams"/> to whitelist allowed properties.
/// </summary>
/// <typeparam name="T">The model type managed by this controller.</typeparam>
[ApiController]
[Route("api/[controller]")]
public abstract class ApiResourceController<T> : ControllerBase where T : Model<T>, new()
{
    private const int DefaultPageSize = 25;
    private const int MaxPageSize = 100;

    /// <summary>The database context for accessing and persisting entities.</summary>
    protected VoltDbContext Db { get; }

    /// <summary>Property names allowed in create and update operations.</summary>
    public abstract string[] PermittedParams { get; }

    /// <summary>Initializes the controller with the given database context.</summary>
    protected ApiResourceController(VoltDbContext db) => Db = db;

    /// <summary>GET /api/resources - Lists entities with pagination metadata.</summary>
    [HttpGet]
    public virtual async Task<IResult> Index(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = DefaultPageSize)
    {
        var normalizedPage = Math.Max(1, page);
        var normalizedSize = Math.Clamp(pageSize, 1, MaxPageSize);
        var query = Db.Set<T>().AsNoTracking();
        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((normalizedPage - 1) * normalizedSize)
            .Take(normalizedSize)
            .ToListAsync();

        return Results.Ok(new
        {
            data = items,
            meta = new
            {
                total = totalCount,
                page = normalizedPage,
                pageSize = normalizedSize,
                totalPages = (int)Math.Ceiling((double)totalCount / normalizedSize)
            }
        });
    }

    /// <summary>GET /api/resources/{id} - Returns a single entity as JSON.</summary>
    [HttpGet("{id:int}")]
    public virtual async Task<IResult> Show(int id)
    {
        var entity = await Find(id);
        if (entity is null)
            return Results.NotFound(new { error = $"{typeof(T).Name} not found." });

        return Results.Ok(new { data = entity });
    }

    /// <summary>POST /api/resources - Creates a new entity from the JSON request body.</summary>
    [HttpPost]
    public virtual async Task<IResult> Create()
    {
        var entity = new T();

        if (!await TryUpdateModelAsync(entity, string.Empty, BuildPropertyIncludes()))
            return Results.BadRequest(new { error = "Invalid request data.", errors = GetModelErrors() });

        if (!ModelState.IsValid)
            return Results.BadRequest(new { error = "Validation failed.", errors = GetModelErrors() });

        Db.Set<T>().Add(entity);
        await Db.SaveChangesAsync();
        return Results.Created($"/api/{GetResourceName()}/{entity.Id}", new { data = entity });
    }

    /// <summary>PUT /api/resources/{id} - Updates an existing entity.</summary>
    [HttpPut("{id:int}")]
    public virtual async Task<IResult> Update(int id)
    {
        var entity = await Find(id);
        if (entity is null)
            return Results.NotFound(new { error = $"{typeof(T).Name} not found." });

        if (!await TryUpdateModelAsync(entity, string.Empty, BuildPropertyIncludes()))
            return Results.BadRequest(new { error = "Invalid request data.", errors = GetModelErrors() });

        if (!ModelState.IsValid)
            return Results.BadRequest(new { error = "Validation failed.", errors = GetModelErrors() });

        await Db.SaveChangesAsync();
        return Results.Ok(new { data = entity });
    }

    /// <summary>DELETE /api/resources/{id} - Removes an entity.</summary>
    [HttpDelete("{id:int}")]
    public virtual async Task<IResult> Destroy(int id)
    {
        var entity = await Find(id);
        if (entity is null)
            return Results.NotFound(new { error = $"{typeof(T).Name} not found." });

        Db.Set<T>().Remove(entity);
        await Db.SaveChangesAsync();
        return Results.NoContent();
    }

    /// <summary>Finds an entity by its primary key identifier.</summary>
    protected virtual async Task<T?> Find(int id) =>
        await Db.Set<T>().FindAsync(id);

    private Expression<Func<T, object?>>[] BuildPropertyIncludes()
    {
        var parameter = Expression.Parameter(typeof(T), "x");

        return PermittedParams
            .Select(name =>
            {
                var property = Expression.Property(parameter, name);
                var converted = Expression.Convert(property, typeof(object));
                return Expression.Lambda<Func<T, object?>>(converted, parameter);
            })
            .ToArray();
    }

    private Dictionary<string, string[]> GetModelErrors() =>
        ModelState
            .Where(kvp => kvp.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

    private string GetResourceName()
    {
        var controllerName = GetType().Name
            .Replace("Controller", string.Empty, StringComparison.OrdinalIgnoreCase);
        return controllerName.ToLowerInvariant();
    }
}
