using DocumentManagementSystem.Mappings;
using DocumentManagementSystem.DTOs;
using FluentValidation;
using FluentValidation.AspNetCore;
using dms_dal_new.Data;
using dms_bl.Services;
using Microsoft.EntityFrameworkCore;
using dms_dal_new.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

//Mapping
builder.Services.AddAutoMapper(typeof(MappingProfile));

//FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<DocumentDTOValidator>();
  
builder.Services.AddDbContext<DocumentContext>(options =>
options.UseNpgsql(builder.Configuration.GetConnectionString("DocumentDatabase")));
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IDocumentService, DocumentService>(); //give it document repository

// CORS konfigurieren, um Anfragen von localhost:80 (WebUI) zuzulassen
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebUI",
        policy =>
        {
            policy.WithOrigins("http://localhost") // Die URL deiner Web-UI
                .AllowAnyHeader()
                .AllowAnyOrigin()
                .AllowAnyMethod();
        });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    c.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    //swagger unter http://localhost:8081/swagger/index.html fixieren (sichert gegen Konflikte durch nginx oder browser-cache oder Konfigurationsprobleme)
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
    c.RoutePrefix = "swagger";
});

// Verwende die CORS-Policy
app.UseCors("AllowWebUI");

//app.UseHttpsRedirection();

// Explicitly listen to HTTP only
app.Urls.Add("http://*:8081"); // Stelle sicher, dass die App nur HTTP verwendet
app.UseAuthorization();

app.MapControllers();

app.Run();