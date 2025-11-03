using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.ModelBuilder;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using TodoApi.Models;

namespace TodoApi
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // Add Entity Framework with In-Memory Database
            services.AddDbContext<TodoContext>(options =>
                options.UseInMemoryDatabase("TodoApi"));

            // Add Controllers with JSON options and OData
            services.AddControllers()
                    .AddOData(options => options
                    .Select()
                    .Expand()
                    .Filter()
                    .OrderBy()
                    .SetMaxTop(100)
                    .Count()
                    .AddRouteComponents("odata", GetEdmModel()))
                    .AddJsonOptions(options =>
                    {
                        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                    });

            // Add API Explorer for Swagger - MUST be after AddControllers
            services.AddEndpointsApiExplorer();

            // Add Swagger with explicit OpenAPI 3.0.1 specification
            services.AddSwaggerGen(c =>
            {
                // Define the API document
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "TodoApi",
                    Description = "A simple Todo API with OData support",
                    Contact = new OpenApiContact
                    {
                        Name = "Serhii Yasenev",
                        Url = new Uri("https://github.com/serhiiyasenev")
                    }
                });

                // Include all APIs - this is important
                c.DocInclusionPredicate((name, api) => true);
      
                // Describe all parameters in camelCase
                c.DescribeAllParametersInCamelCase();

                // Support for OData query parameters
                c.OperationFilter<ODataOperationFilter>();
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            // Enable Swagger middleware (OpenAPI 3.0 is default in Swashbuckle 9.x)
            app.UseSwagger();
            
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = string.Empty;
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "TodoApi v1");
                c.DocumentTitle = "TodoApi Documentation";
            });

            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private static Microsoft.OData.Edm.IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();

            builder.EntitySet<TodoItem>(nameof(TodoItem))
                   .EntityType
                   .HasKey(x => x.Id);

            builder.EntitySet<TodoItemValue>(nameof(TodoItemValue))
                   .EntityType
                   .HasKey(x => x.Id);

            return builder.GetEdmModel();
        }
    }
}