using Microsoft.EntityFrameworkCore;

namespace dms_dal
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            /*builder.Services.AddDbContext<TodoContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("TodoDatabase")));

            builder.Services.AddScoped<ITodoItemRepository, TodoItemRepository>();*/

            var app = builder.Build();

            // Migrations und Datenbankerstellung anwenden
            /*using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<TodoContext>();

                // Verbindungstest zur Datenbank
                try
                {
                    Console.WriteLine("Versuche, eine Verbindung zur Datenbank herzustellen...");

                    // Warte, bis die Datenbank bereit ist
                    while (!context.Database.CanConnect())
                    {
                        Console.WriteLine("Datenbank ist noch nicht bereit, warte...");
                        Thread.Sleep(1000); // Warte 1 Sekunde
                    }

                    Console.WriteLine("Verbindung zur Datenbank erfolgreich.");

                    // Migrations anwenden und die Datenbank erstellen/aktualisieren
                    context.Database.EnsureCreated();
                    Console.WriteLine("Datenbankmigrationen erfolgreich angewendet.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fehler bei der Anwendung der Migrationen: {ex.Message}");
                }
            }*/

            app.MapControllers();

            app.Run();
        }
    }
}
