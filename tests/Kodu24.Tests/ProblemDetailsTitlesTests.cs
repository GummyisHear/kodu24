using Kodu24.Api.Errors;
using Microsoft.AspNetCore.Mvc;

namespace Kodu24.Tests;

public class ProblemDetailsTitlesTests
{
    [Fact]
    public void Apply_ReplacesDefaultEnglishTitle()
    {
        var problem = new ProblemDetails { Status = 404, Title = "Not Found" };

        ProblemDetailsTitles.Apply(problem);

        Assert.Equal("Ressurssi ei leitud", problem.Title);
    }

    [Fact]
    public void Apply_ValidationProblem_GetsValidationTitle()
    {
        var problem = new ValidationProblemDetails { Status = 400, Title = "One or more validation errors occurred." };

        ProblemDetailsTitles.Apply(problem);

        Assert.Equal(ProblemDetailsTitles.ValidationTitle, problem.Title);
    }

    [Fact]
    public void Apply_KeepsCustomTitle()
    {
        var problem = new ProblemDetails { Status = 401, Title = "Autentimine ebaõnnestus" };

        ProblemDetailsTitles.Apply(problem);

        Assert.Equal("Autentimine ebaõnnestus", problem.Title);
    }
}
