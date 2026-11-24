using Kodu24.Api.Entities;

namespace Kodu24.Api.Auth;

public interface ITokenService
{
    /// <summary>Loob kasutajale allkirjastatud JWT ja tagastab selle koos aegumisajaga.</summary>
    (string Token, DateTime ExpiresAt) CreateToken(User user);
}
