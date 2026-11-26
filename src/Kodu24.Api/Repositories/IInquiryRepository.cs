using Kodu24.Api.Entities;

namespace Kodu24.Api.Repositories;

public interface IInquiryRepository : IRepository<Inquiry>
{
    /// <summary>Päring koos kuulutuse ja saatjaga.</summary>
    Task<Inquiry?> GetWithDetailsAsync(int id);

    /// <summary>
    /// Üürileandja kuulutustele saabunud päringud, uuemad eespool.
    /// <paramref name="landlordId"/> = null tagastab kõik (admin).
    /// </summary>
    Task<List<Inquiry>> GetReceivedAsync(int? landlordId);
}
