var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://*:8081");  // Specify the URL to listen on

// Use the Startup class to configure services and application
builder.Services.AddSingleton<Startup>();

var app = builder.Build();

// Enable static files middleware
app.UseStaticFiles();

// Use the Startup class to configure the app's middleware
var startup = app.Services.GetRequiredService<Startup>();
startup.ConfigureServices(builder.Services);
startup.Configure(app);

// Enable routing middleware
app.UseRouting();

// Map controller routes
app.MapControllers();  // This should be correctly mapped here

app.Run();
