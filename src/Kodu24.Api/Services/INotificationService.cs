using Kodu24.Api.Auth;
using Kodu24.Api.Dtos;

namespace Kodu24.Api.Services;

public interface INotificationService
{
    /// <summary>
    /// Taustateenus: loob otsingu omanikule teavituse iga uue kuulutuse kohta.
    /// Puuduvad/kustutatud kuulutused ja juba teavitatud kuulutused jäetakse vahele.
    /// </summary>
    Task<CreateNotificationsResultDto> CreateForSavedSearchAsync(CreateNotificationsRequestDto request);

    /// <summary>Kasutaja enda teavitused, uuemad eespool.</summary>
    Task<List<NotificationDto>> GetMineAsync(CurrentUser user);

    Task MarkAsReadAsync(int id, CurrentUser user);
}
