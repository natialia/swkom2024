var builder = WebApplication.CreateBuilder(args);

// Füge statische Dateien hinzu
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Aktiviere die Verwendung von statischen Dateien
app.UseStaticFiles();

// Aktiviere den Routing-Support
app.UseRouting();

app.MapGet("/", () => Results.Redirect("/index.html")); // Weiterleitung auf die Hauptseite

app.Run();