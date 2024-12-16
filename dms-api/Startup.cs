using DocumentManagementSystem.Mappings;
using DocumentManagementSystem.DTOs;
using FluentValidation;
using FluentValidation.AspNetCore;
using dms_dal_new.Data;
using dms_bl.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using dms_dal_new.Repositories;
using Npgsql;
using dms_bl.Validators;
using DocumentManagementSystem.Controllers;
using Serilog;
using Minio;
using System.Diagnostics.CodeAnalysis;
[ExcludeFromCodeCoverage]
public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        // Serilog logging
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        Log.Information("Starting web application");

        // Add services to the container
        services.AddSerilog();

        // Add controllers and views
        services.AddControllersWithViews();

        // Add AutoMapper
        services.AddAutoMapper(typeof(MappingProfile));

        // Add FluentValidation
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<DocumentDTOValidator>();
        services.AddValidatorsFromAssemblyContaining<DocumentValidator>();

        // Database configuration
        services.AddDbContext<DocumentContext>(options =>
            options.UseNpgsql(Configuration.GetConnectionString("DocumentDatabase")));

        // Repositories and services
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IDocumentLogic, DocumentLogic>();

        // Minio client configuration
        services.AddScoped<IMinioClient>(s =>
        {
            return new MinioClient()
                .WithEndpoint("minio", 9000)
                .WithCredentials("minioadmin", "minioadmin")
                .WithSSL(false)
                .Build();
        });

        // ElasticSearch configuration
        services.AddScoped<IElasticSearchClientAgent, ElasticSearchClientAgent>();

        // CORS configuration
        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
                policy.WithOrigins("http://localhost")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials());
        });

        // Register HttpClient for RabbitMQ listener
        services.AddHttpClient("dms-api", client =>
        {
            client.BaseAddress = new Uri("http://dms-api:8081");
        });

        // Add RabbitMQ background service
        services.AddSingleton<IMessageQueueService, MessageQueueService>();
        services.AddHostedService<RabbitMqListenerService>();

        // Swagger configuration
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
        });
    }

    public async Task Configure(IApplicationBuilder app)
    {
        // Enable Serilog request logging
        app.UseSerilogRequestLogging();

        // Use migrations and database creation logic
        using (var scope = app.ApplicationServices.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<DocumentContext>();

            // Database connection test
            try
            {
                Console.WriteLine("Attempting to connect to the database...");
                while (true)
                {
                    try
                    {
                        await context.Database.OpenConnectionAsync();
                        context.Database.CloseConnection();
                        Console.WriteLine("Successfully connected to the database.");
                        break;
                    }
                    catch (NpgsqlException)
                    {
                        Console.WriteLine("Database is not ready, waiting...");
                        await Task.Delay(1000);
                    }
                }

                // Apply migrations and ensure database is created
                context.Database.EnsureCreated();
                Console.WriteLine("Database migrations successfully applied.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error applying migrations: {ex.Message}");
            }
        }

        // Swagger and CORS configuration
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
            c.RoutePrefix = "swagger";
        });

        app.UseCors("AllowFrontend");

        // Explicitly listen to HTTP only
        app.UseAuthorization();
        app.UseStaticFiles();
    }
}
