using Microsoft.AspNetCore.OData.Query;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TodoApi
{
    public class ODataOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null) return;

            // Add OData query parameters for GET operations with EnableQuery attribute
            if (context.ApiDescription.HttpMethod == "GET")
            {
                var enableQueryAttribute = context.MethodInfo
                 .GetCustomAttributes(true)
                   .OfType<EnableQueryAttribute>()
                   .FirstOrDefault();

                if (enableQueryAttribute != null)
                {
                    operation.Parameters.Add(new OpenApiParameter
                    {
                        Name = "$filter",
                        In = ParameterLocation.Query,
                        Schema = new OpenApiSchema { Type = "string" },
                        Description = "Filter the results using OData syntax.",
                        Required = false
                    });

                    operation.Parameters.Add(new OpenApiParameter
                    {
                        Name = "$orderby",
                        In = ParameterLocation.Query,
                        Schema = new OpenApiSchema { Type = "string" },
                        Description = "Order the results using OData syntax.",
                        Required = false
                    });

                    operation.Parameters.Add(new OpenApiParameter
                    {
                        Name = "$top",
                        In = ParameterLocation.Query,
                        Schema = new OpenApiSchema { Type = "integer" },
                        Description = "The max number of records.",
                        Required = false
                    });

                    operation.Parameters.Add(new OpenApiParameter
                    {
                        Name = "$skip",
                        In = ParameterLocation.Query,
                        Schema = new OpenApiSchema { Type = "integer" },
                        Description = "The number of records to skip.",
                        Required = false
                    });

                    operation.Parameters.Add(new OpenApiParameter
                    {
                        Name = "$count",
                        In = ParameterLocation.Query,
                        Schema = new OpenApiSchema { Type = "boolean" },
                        Description = "Return the total count of records.",
                        Required = false
                    });

                    operation.Parameters.Add(new OpenApiParameter
                    {
                        Name = "$expand",
                        In = ParameterLocation.Query,
                        Schema = new OpenApiSchema { Type = "string" },
                        Description = "Expand related entities using OData syntax.",
                        Required = false
                    });

                    operation.Parameters.Add(new OpenApiParameter
                    {
                        Name = "$select",
                        In = ParameterLocation.Query,
                        Schema = new OpenApiSchema { Type = "string" },
                        Description = "Select specific properties using OData syntax.",
                        Required = false
                    });
                }
            }
        }
    }
}
