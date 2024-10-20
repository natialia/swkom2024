using Microsoft.EntityFrameworkCore;
using dms_dal.Data;
using dms_dal.Repositories;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration //because appsettings was renamed
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("dal-appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<DocumentContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DocumentDatabase")));

builder.Services.AddScoped<IDocumentItemRepository, DocumentItemRepository>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Migrations und Datenbankerstellung anwenden
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DocumentContext>();

    // Verbindungstest zur Datenbank
    try
    {
        Console.WriteLine("Versuche, eine Verbindung zur Datenbank herzustellen...");

        // Wait until the database is ready
        while (true)
        {
            try
            {
                await context.Database.OpenConnectionAsync();
                context.Database.CloseConnection();
                Console.WriteLine("Verbindung zur Datenbank erfolgreich.");
                break; // Exit the loop if the connection is successful
            }
            catch (NpgsqlException)
            {
                Console.WriteLine("Datenbank ist noch nicht bereit, warte...");
                await Task.Delay(1000); // Wait 1 second before retrying
            }
        }

        // Migrations anwenden und die Datenbank erstellen/aktualisieren
        context.Database.EnsureCreated();
        Console.WriteLine("Datenbankmigrationen erfolgreich angewendet.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Fehler bei der Anwendung der Migrationen: {ex.Message}");
    }
}
// Swagger-Middleware hinzufügen
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "YourAPI v1"));
}

app.MapControllers();

app.Run();