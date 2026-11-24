namespace Kodu24.Api.Exceptions;

/// <summary>
/// Teenuste kihis visatav viga, mille <see cref="Errors.GlobalExceptionHandler"/>
/// muudab ProblemDetails vastuseks vastava HTTP staatusega.
/// </summary>
public abstract class ApiException : Exception
{
    protected ApiException(int statusCode, string title, string detail) : base(detail)
    {
        StatusCode = statusCode;
        Title = title;
    }

    public int StatusCode { get; }
    public string Title { get; }
}

/// <summary>
/// Äriloogika valideerimisviga, mida DTO atribuudid ei saa kontrollida (nt kas linnaosa on olemas).
/// Tagastatakse samas kujus nagu DTO valideerimine: 400 + väljade kaupa <c>errors</c>.
/// </summary>
public class ValidationFailedException : ApiException
{
    public ValidationFailedException(string field, string message)
        : base(StatusCodes.Status400BadRequest, "Sisendandmed on vigased", message)
    {
        Errors = new Dictionary<string, string[]> { [field] = [message] };
    }

    public IDictionary<string, string[]> Errors { get; }
}

public class NotFoundException(string detail)
    : ApiException(StatusCodes.Status404NotFound, "Ressurssi ei leitud", detail);

public class UnauthorizedException(string detail)
    : ApiException(StatusCodes.Status401Unauthorized, "Autentimine ebaõnnestus", detail);

public class ForbiddenException(string detail)
    : ApiException(StatusCodes.Status403Forbidden, "Juurdepääs keelatud", detail);

public class ConflictException(string detail)
    : ApiException(StatusCodes.Status409Conflict, "Konflikt", detail);
