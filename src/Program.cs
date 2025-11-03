using TodoApi;
using TodoApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure services using Startup class
var startup = new Startup();
startup.ConfigureServices(builder.Services);

var app = builder.Build();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<TodoContext>();
    Initializer.Initialize(context);
}

// Configure middleware using Startup class
startup.Configure(app);

app.Run();

// Make the implicit Program class public for testing
public partial class Program { }