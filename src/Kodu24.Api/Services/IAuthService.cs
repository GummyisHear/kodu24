using Kodu24.Api.Dtos;

namespace Kodu24.Api.Services;

public interface IAuthService
{
    /// <summary>Loob uue kasutaja ja tagastab tokeni. Hõivatud e-post → <c>ConflictException</c>.</summary>
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);

    /// <summary>Kontrollib parooli ja tagastab tokeni. Valed andmed → <c>UnauthorizedException</c>.</summary>
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);

    /// <summary>Kasutaja andmed ID järgi (nt tokenist). Puudub → <c>NotFoundException</c>.</summary>
    Task<UserDto> GetUserAsync(int userId);
}
