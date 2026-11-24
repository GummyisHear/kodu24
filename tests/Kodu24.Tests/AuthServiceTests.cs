using Kodu24.Api.Auth;
using Kodu24.Api.Dtos;
using Kodu24.Api.Entities;
using Kodu24.Api.Exceptions;
using Kodu24.Api.Repositories;
using Kodu24.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Kodu24.Tests;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<ITokenService> _tokens = new();

    public AuthServiceTests()
    {
        _tokens.Setup(t => t.CreateToken(It.IsAny<User>())).Returns(("test-token", DateTime.UtcNow.AddHours(1)));
    }

    private AuthService CreateService() => new(_users.Object, _tokens.Object, NullLogger<AuthService>.Instance);

    private static RegisterRequestDto Request(string email = "Kati@Kodu24.ee") => new()
    {
        Email = email,
        Password = "Parool123!",
        FullName = "Kati Karu",
        Role = "Seeker",
    };

    [Fact]
    public async Task RegisterAsync_HashesPasswordAndNormalizesEmail()
    {
        User? saved = null;
        _users.Setup(u => u.AddAsync(It.IsAny<User>())).Callback<User>(u => saved = u);

        var result = await CreateService().RegisterAsync(Request());

        Assert.NotNull(saved);
        Assert.Equal("kati@kodu24.ee", saved.Email);
        Assert.NotEqual("Parool123!", saved.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("Parool123!", saved.PasswordHash));
        Assert.Equal(UserRole.Seeker, saved.Role);
        Assert.Equal("test-token", result.Token);
        _users.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsConflict()
    {
        _users.Setup(u => u.GetByEmailAsync("kati@kodu24.ee")).ReturnsAsync(new User { Email = "kati@kodu24.ee" });

        await Assert.ThrowsAsync<ConflictException>(() => CreateService().RegisterAsync(Request()));
        _users.Verify(u => u.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorized()
    {
        _users.Setup(u => u.GetByEmailAsync("liis@kodu24.ee")).ReturnsAsync(new User
        {
            Email = "liis@kodu24.ee",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Parool123!"),
        });

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            CreateService().LoginAsync(new LoginRequestDto { Email = "liis@kodu24.ee", Password = "vale-parool" }));
    }

    [Fact]
    public async Task LoginAsync_UnknownEmail_ThrowsUnauthorized()
    {
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            CreateService().LoginAsync(new LoginRequestDto { Email = "pole@kodu24.ee", Password = "Parool123!" }));
    }
}
