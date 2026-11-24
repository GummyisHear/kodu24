using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace Kodu24.Api.Errors;

/// <summary>
/// Asendab raamistiku ingliskeelsed vaikepealkirjad (nt "Not Found",
/// "One or more validation errors occurred.") eestikeelsetega.
/// Teenuste kihi enda pealkirju ei muudeta.
/// </summary>
public static class ProblemDetailsTitles
{
    public const string ValidationTitle = "Sisendandmed on vigased";

    private static readonly Dictionary<int, string> Titles = new()
    {
        [400] = "Vigane päring",
        [401] = "Autentimine on nõutud",
        [403] = "Juurdepääs keelatud",
        [404] = "Ressurssi ei leitud",
        [405] = "Meetod ei ole lubatud",
        [409] = "Konflikt",
        [415] = "Toetamata andmevorming",
        [500] = "Serveris tekkis ootamatu viga",
    };

    public static void Apply(ProblemDetails problem)
    {
        if (problem is HttpValidationProblemDetails)
        {
            problem.Title = ValidationTitle;
            return;
        }

        if (problem.Status is not int status || !Titles.TryGetValue(status, out var title))
            return;

        var isDefaultTitle = string.IsNullOrEmpty(problem.Title)
                             || problem.Title == ReasonPhrases.GetReasonPhrase(status)
                             || problem.Title == "An error occurred while processing your request.";
        if (isDefaultTitle)
            problem.Title = title;
    }
}
