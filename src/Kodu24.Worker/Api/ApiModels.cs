namespace Kodu24.Worker.Api;

// Taustateenuse enda koopiad API vastustest. Worker ei viita API projektile ega näe kunagi entiteete:
// siin on ainult need väljad, mida taustateenus vajab.

public record LoginResponse(string Token, DateTime ExpiresAt);

public record ExpireResult(int ExpiredCount);

public record SavedSearch(
    int Id,
    int UserId,
    string Name,
    decimal? MinPrice,
    decimal? MaxPrice,
    int? Rooms,
    int? DistrictId,
    DateTime LastNotifiedAt);

public record ListingItem(int Id, string Title, DateTime CreatedAt);

public record ListingPage(List<ListingItem> Items, int Page, int TotalPages);

public record CreateNotificationsResult(int CreatedCount, int SkippedCount);
