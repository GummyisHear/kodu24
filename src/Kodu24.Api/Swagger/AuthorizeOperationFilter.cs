using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Kodu24.Api.Swagger;

/// <summary>
/// Lisab Swaggeris luku ikooni (Bearer nõude) ainult neile otspunktidele,
/// millel on <c>[Authorize]</c> ja mitte <c>[AllowAnonymous]</c>.
/// </summary>
public class AuthorizeOperationFilter : IOperationFilter
{
    public const string SchemeName = "Bearer";

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;
        var requiresAuth = metadata.OfType<IAuthorizeData>().Any()
                           && !metadata.OfType<IAllowAnonymous>().Any();
        if (!requiresAuth)
            return;

        operation.Security ??= [];
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(SchemeName, context.Document)] = [],
        });
    }
}
