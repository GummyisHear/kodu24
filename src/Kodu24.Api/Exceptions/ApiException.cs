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

public class NotFoundException(string detail)
    : ApiException(StatusCodes.Status404NotFound, "Ressurssi ei leitud", detail);

public class ForbiddenException(string detail)
    : ApiException(StatusCodes.Status403Forbidden, "Juurdepääs keelatud", detail);

public class ConflictException(string detail)
    : ApiException(StatusCodes.Status409Conflict, "Konflikt", detail);
