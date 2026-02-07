using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Volt.Core;
using Volt.Core.Flash;
using Volt.Data;

namespace Volt.Web.Controllers;

/// <summary>
/// Base controller providing conventional CRUD actions for a resource.
/// Override <see cref="PermittedParams"/> to whitelist allowed properties.
/// </summary>
/// <typeparam name="T">The model type managed by this controller.</typeparam>
public abstract class ResourceController<T> : Controller where T : Model<T>, new()
{
    private const string FlashTempDataKey = "_VoltFlash";

    /// <summary>The database context for accessing and persisting entities.</summary>
    protected VoltDbContext Db { get; }

    /// <summary>Property names allowed in create and update operations.</summary>
    public abstract string[] PermittedParams { get; }

    /// <summary>Initializes the controller with the given database context.</summary>
    protected ResourceController(VoltDbContext db) => Db = db;

    /// <summary>GET /resources - Lists all entities.</summary>
    [HttpGet]
    public virtual async Task<IActionResult> Index()
    {
        var items = await Db.Set<T>().AsNoTracking().ToListAsync();
        return View(items);
    }

    /// <summary>GET /resources/{id} - Displays a single entity.</summary>
    [HttpGet("{id:int}")]
    public virtual async Task<IActionResult> Show(int id)
    {
        var entity = await Find(id);
        return entity is null ? NotFound() : View(entity);
    }

    /// <summary>GET /resources/new - Displays the form for creating a new entity.</summary>
    [HttpGet("new")]
    public virtual IActionResult New() => View(new T());

    /// <summary>POST /resources - Creates a new entity from the submitted form data.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public virtual async Task<IActionResult> Create()
    {
        var entity = new T();

        if (!await TryUpdateModelAsync(entity, string.Empty, BuildPropertyIncludes()))
            return View("New", entity);

        if (!ModelState.IsValid)
            return View("New", entity);

        Db.Set<T>().Add(entity);
        await Db.SaveChangesAsync();
        Flash("Created successfully.", FlashMessageType.Success);
        return RedirectTo(entity);
    }

    /// <summary>GET /resources/{id}/edit - Displays the edit form for an existing entity.</summary>
    [HttpGet("{id:int}/edit")]
    public virtual async Task<IActionResult> Edit(int id)
    {
        var entity = await Find(id);
        return entity is null ? NotFound() : View(entity);
    }

    /// <summary>PUT /resources/{id} - Updates an existing entity.</summary>
    [HttpPut("{id:int}")]
    [ValidateAntiForgeryToken]
    public virtual async Task<IActionResult> Update(int id)
    {
        var entity = await Find(id);
        if (entity is null) return NotFound();

        if (!await TryUpdateModelAsync(entity, string.Empty, BuildPropertyIncludes()))
            return View("Edit", entity);

        if (!ModelState.IsValid)
            return View("Edit", entity);

        await Db.SaveChangesAsync();
        Flash("Updated successfully.", FlashMessageType.Success);
        return RedirectTo(entity);
    }

    /// <summary>DELETE /resources/{id} - Removes an existing entity.</summary>
    [HttpDelete("{id:int}")]
    [ValidateAntiForgeryToken]
    public virtual async Task<IActionResult> Destroy(int id)
    {
        var entity = await Find(id);
        if (entity is null) return NotFound();

        Db.Set<T>().Remove(entity);
        await Db.SaveChangesAsync();
        Flash("Deleted successfully.", FlashMessageType.Success);
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Finds an entity by its primary key identifier.</summary>
    protected virtual async Task<T?> Find(int id) =>
        await Db.Set<T>().FindAsync(id);

    /// <summary>Extracts the route id parameter from the current request.</summary>
    protected int RouteId =>
        int.TryParse(RouteData.Values["id"]?.ToString(), out var id) ? id : 0;

    /// <summary>Redirects to the Show action for the specified entity.</summary>
    protected IActionResult RedirectTo(T entity, string? flash = null)
    {
        if (flash is not null)
            Flash(flash, FlashMessageType.Success);

        return RedirectToAction(nameof(Show), new { id = entity.Id });
    }

    /// <summary>Sets a flash message in TempData to be displayed after redirect.</summary>
    protected void Flash(string message, FlashMessageType type)
    {
        var flashMessage = new FlashMessage(type, message);
        TempData[FlashTempDataKey] = JsonSerializer.Serialize(flashMessage);
    }

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
}
