using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Kodu24.Api.Swagger;

/// <summary>
/// Otspunktidele, millel on <c>[Authorize]</c> (ja mitte <c>[AllowAnonymous]</c>):
/// lisab Swaggeris luku ikooni (Bearer nõude), 401/403 vastused ja märke, milline ligipääs on nõutud.
/// </summary>
public class AuthorizeOperationFilter : IOperationFilter
{
    public const string SchemeName = "Bearer";

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;
        var authorize = metadata.OfType<IAuthorizeData>().ToList();
        if (authorize.Count == 0 || metadata.OfType<IAllowAnonymous>().Any())
            return;

        operation.Security ??= [];
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(SchemeName, context.Document)] = [],
        });

        operation.Responses ??= new OpenApiResponses();
        operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Token puudub või on aegunud." });

        var requirements = authorize
            .SelectMany(a => new[] { a.Policy, a.Roles })
            .Where(r => !string.IsNullOrEmpty(r))
            .Distinct()
            .ToList();
        if (requirements.Count == 0)
            return;

        operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Sinu roll ei luba seda toimingut." });
        operation.Description = $"{operation.Description}\n\n**Nõutud ligipääs:** {string.Join(", ", requirements)}".Trim();
    }
}
