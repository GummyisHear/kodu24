using Kodu24.Api.Errors;
using Kodu24.Api.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Kodu24.Tests;

public class GlobalExceptionHandlerTests
{
    private static (GlobalExceptionHandler Handler, Mock<IProblemDetailsService> Service) Create()
    {
        var service = new Mock<IProblemDetailsService>();
        service.Setup(s => s.TryWriteAsync(It.IsAny<ProblemDetailsContext>())).ReturnsAsync(true);
        return (new GlobalExceptionHandler(service.Object, NullLogger<GlobalExceptionHandler>.Instance), service);
    }

    [Fact]
    public async Task ApiException_WritesItsStatusAndMessage()
    {
        var (handler, service) = Create();
        var context = new DefaultHttpContext();

        await handler.TryHandleAsync(context, new NotFoundException("Kuulutust 5 ei leitud"), CancellationToken.None);

        Assert.Equal(404, context.Response.StatusCode);
        service.Verify(s => s.TryWriteAsync(It.Is<ProblemDetailsContext>(p =>
            p.ProblemDetails.Status == 404 &&
            p.ProblemDetails.Title == "Ressurssi ei leitud" &&
            p.ProblemDetails.Detail == "Kuulutust 5 ei leitud")), Times.Once);
    }

    [Fact]
    public async Task UnexpectedException_Returns500WithoutLeakingDetails()
    {
        var (handler, service) = Create();
        var context = new DefaultHttpContext();

        await handler.TryHandleAsync(context, new InvalidOperationException("salajane sisu"), CancellationToken.None);

        Assert.Equal(500, context.Response.StatusCode);
        service.Verify(s => s.TryWriteAsync(It.Is<ProblemDetailsContext>(p =>
            p.ProblemDetails.Status == 500 &&
            p.ProblemDetails.Detail == null)), Times.Once);
    }
}
