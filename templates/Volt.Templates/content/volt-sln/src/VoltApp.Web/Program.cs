using Volt.Data;
using Volt.Storage.Extensions;
using Volt.Web.Middleware;
using VoltApp.Data;

var builder = WebApplication.CreateBuilder(args);

// Add Volt services
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>();
builder.Services.AddScoped<VoltDbContext>(sp => sp.GetRequiredService<AppDbContext>());
builder.Services.AddVoltStorage();

var app = builder.Build();

// Configure the Volt pipeline
app.UseVolt();

app.Run();
