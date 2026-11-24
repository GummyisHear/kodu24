using Kodu24.Api.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Kodu24.Api.Errors;

/// <summary>
/// Püüab kõik käsitlemata vead ja tagastab need ühtse ProblemDetails vastusena.
/// Ootamatute vigade (500) sisu kliendile ei näidata, see läheb ainult logisse.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetails;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(IProblemDetailsService problemDetails, ILogger<GlobalExceptionHandler> logger)
    {
        _problemDetails = problemDetails;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        ProblemDetails problem = exception is ValidationFailedException validation
            ? new HttpValidationProblemDetails(validation.Errors)
            : new ProblemDetails();

        if (exception is ApiException api)
        {
            problem.Status = api.StatusCode;
            problem.Title = api.Title;
            problem.Detail = api.Message;
            _logger.LogWarning("{Title}: {Detail}", api.Title, api.Message);
        }
        else
        {
            problem.Status = StatusCodes.Status500InternalServerError;
            problem.Title = "Serveris tekkis ootamatu viga";
            _logger.LogError(exception, "Käsitlemata viga päringul {Method} {Path}",
                httpContext.Request.Method, httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = problem.Status.Value;

        return await _problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problem,
        });
    }
}
