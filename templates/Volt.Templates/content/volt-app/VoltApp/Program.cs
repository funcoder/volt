using VoltApp.Data;

var builder = WebApplication.CreateBuilder(args);

// Add Volt services
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>();

var app = builder.Build();

// Configure the Volt pipeline
app.UseVolt();

app.Run();
