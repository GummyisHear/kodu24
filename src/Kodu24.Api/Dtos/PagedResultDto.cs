namespace Kodu24.Api.Dtos;

/// <summary>Üks lehekülg tulemusi koos kogusummaga.</summary>
/// <param name="Items">Selle lehekülje kirjed.</param>
/// <param name="Page">Lehekülje number (algab 1-st).</param>
/// <param name="PageSize">Kirjeid leheküljel.</param>
/// <param name="TotalCount">Kõigi filtrile vastavate kirjete arv.</param>
public record PagedResultDto<T>(List<T> Items, int Page, int PageSize, int TotalCount)
{
    /// <summary>Lehekülgede arv kokku.</summary>
    public int TotalPages => TotalCount == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
