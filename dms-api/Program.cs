using DocumentManagementSystem.Mappings;
using DocumentManagementSystem.DTOs;
using FluentValidation;
using FluentValidation.AspNetCore;
using dms_dal_new.Data;
using dms_bl.Services;
using Microsoft.EntityFrameworkCore;
using dms_dal_new.Repositories;
using Npgsql;
using dms_bl.Validators;
using Elastic.Clients.Elasticsearch;
using DocumentManagementSystem.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

//Mapping
builder.Services.AddAutoMapper(typeof(MappingProfile));

//FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<DocumentDTOValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<DocumentValidator>();

builder.Services.AddDbContext<DocumentContext>(options =>
options.UseNpgsql(builder.Configuration.GetConnectionString("DocumentDatabase")));
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>(); // Everything must be singleton, so that hosted service can register dependency
builder.Services.AddScoped<IDocumentLogic, DocumentLogic>();

//ElasticSearch
builder.Services.AddScoped<IElasticSearchClientAgent, ElasticSearchClientAgent>();
//Dummy ElasticSearch
//builder.Services.AddScoped<IElasticSearchClientAgent, DummyElasticSearchClient>();



// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()); // Include this if credentials (cookies) are required
});

// Register HttpClient for rabbitmqlistener
builder.Services.AddHttpClient("dms-api", client =>
{
    client.BaseAddress = new Uri("http://dms-api:8081"); // URL des api Services in Docker
});

// Add RabbitMQ Background Service
builder.Services.AddControllers();
builder.Services.AddSingleton<IMessageQueueService, MessageQueueService>(); // Should all use same queueservice
builder.Services.AddHostedService<RabbitMqListenerService>(); // Have queue listener run in the background to consume ocr response


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    c.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// Use Migrations and Database Creation
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DocumentContext>();

    // Database Connection Test
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

        // Use Migrations and create/update Database
        context.Database.EnsureCreated();
        Console.WriteLine("Datenbankmigrationen erfolgreich angewendet.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Fehler bei der Anwendung der Migrationen: {ex.Message}");
    }
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    //swagger unter http://localhost:8081/swagger/index.html fixieren (sichert gegen Konflikte durch nginx oder browser-cache oder Konfigurationsprobleme)
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
    c.RoutePrefix = "swagger";
});

// CORS-Policy
app.UseCors("AllowFrontend");

//app.UseHttpsRedirection();

// Explicitly listen to HTTP only
app.Urls.Add("http://*:8081");
app.UseAuthorization();
app.UseStaticFiles();

app.MapControllers();

app.Run();