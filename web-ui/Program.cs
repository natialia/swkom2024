namespace web_ui
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // F�ge statische Dateien hinzu
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Aktiviere die Verwendung von statischen Dateien
            app.UseStaticFiles();

            // Aktiviere den Routing-Support
            app.UseRouting();

            app.MapGet("/", () => Results.Redirect("/index.html")); // Weiterleitung auf die Hauptseite

            app.Run();
        }
    }
}
