var builder = WebApplication.CreateBuilder(args);

builder.Configuration //because appsettings was renamed
    .SetBasePath(Directory.GetCurrentDirectory()) 
    .AddJsonFile("web-appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

// Füge statische Dateien hinzu
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Aktiviere die Verwendung von statischen Dateien
app.UseStaticFiles();

// Aktiviere den Routing-Support
app.UseRouting();

app.MapGet("/", () => Results.Redirect("/index.html")); // Weiterleitung auf die Hauptseite

app.Run();