using Kodu24.Api.Auth;
using Kodu24.Api.Dtos;
using Kodu24.Api.Entities;
using Kodu24.Api.Exceptions;
using Kodu24.Api.Mapping;
using Kodu24.Api.Repositories;

namespace Kodu24.Api.Services;

public class AuthService : IAuthService
{
    // Sama teade nii vale e-posti kui ka vale parooli korral, et ei saaks välja uurida, millised kontod on olemas.
    private const string InvalidCredentials = "Vale e-post või parool.";

    private readonly IUserRepository _users;
    private readonly ITokenService _tokens;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserRepository users, ITokenService tokens, ILogger<AuthService> logger)
    {
        _users = users;
        _tokens = tokens;
        _logger = logger;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _users.GetByEmailAsync(email) is not null)
            throw new ConflictException("Selle e-postiga kasutaja on juba olemas.");

        // Admin ja Service kontosid ei saa ise registreerida (DTO valideerimine lubab ainult need kaks).
        var role = Enum.Parse<UserRole>(request.Role);

        var user = new User
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName.Trim(),
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
            Role = role,
        };

        await _users.AddAsync(user);
        await _users.SaveChangesAsync();

        _logger.LogInformation("Registreeriti kasutaja {UserId} rolliga {Role}", user.Id, user.Role);
        return CreateResponse(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _users.GetByEmailAsync(request.Email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Ebaõnnestunud sisselogimine e-postiga {Email}", request.Email);
            throw new UnauthorizedException(InvalidCredentials);
        }

        return CreateResponse(user);
    }

    public async Task<UserDto> GetUserAsync(int userId)
    {
        var user = await _users.GetByIdAsync(userId)
                   ?? throw new NotFoundException($"Kasutajat {userId} ei leitud.");
        return user.ToDto();
    }

    private AuthResponseDto CreateResponse(User user)
    {
        var (token, expiresAt) = _tokens.CreateToken(user);
        return new AuthResponseDto(token, expiresAt, user.ToDto());
    }
}
