using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SmsGatekeeper.Models
{
    public class PhoneNumberSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type == typeof(PhoneNumber))
            {
                schema.Type = "string";
                schema.Example = new Microsoft.OpenApi.Any.OpenApiString("01234567890");
            }
        }
    }
}