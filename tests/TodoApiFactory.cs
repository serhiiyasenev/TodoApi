using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TodoApi.Models;

namespace TodoApi.IntegrationTests;

public class TodoApiFactory : WebApplicationFactory<Program>
{
    // Each factory instance gets a unique database name
    private readonly string _databaseName = $"TestDatabase_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                 d => d.ServiceType == typeof(DbContextOptions<TodoContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add a new in-memory database with unique name for this test instance
            services.AddDbContext<TodoContext>(options =>
            {
             options.UseInMemoryDatabase(_databaseName);
            });

            // Build the service provider and create the database
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TodoContext>();

            // Ensure the database is created
            context.Database.EnsureCreated();
        });
    }
}
