using InventorySystem.Application.DTOs;
using InventorySystem.Application.Services;
using InventorySystem.Domain.Entities;
using InventorySystem.Domain.Enums;
using InventorySystem.Domain.Exceptions;
using InventorySystem.Domain.Interfaces;
using Moq;

namespace InventorySystem.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IJwtTokenGenerator> _tokenGenerator = new();

    public AuthServiceTests()
    {
        _unitOfWork.SetupGet(u => u.Users).Returns(_users.Object);
    }

    private AuthService CreateService() => new(_unitOfWork.Object, _passwordHasher.Object, _tokenGenerator.Object);

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsTokenFromGenerator()
    {
        var user = new User { Id = 1, Username = "admin", PasswordHash = "hashed", Role = UserRole.Admin, IsActive = true };
        _users.Setup(r => r.GetByUsernameAsync("admin")).ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify("hashed", "correct-password")).Returns(true);
        _tokenGenerator.Setup(g => g.GenerateToken(user)).Returns(("jwt-token", new DateTime(2026, 1, 1)));

        var result = await CreateService().LoginAsync(new LoginDto { Username = "admin", Password = "correct-password" });

        Assert.Equal("jwt-token", result.Token);
        Assert.Equal("admin", result.Username);
        Assert.Equal("Admin", result.Role);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsAuthenticationException()
    {
        var user = new User { Id = 1, Username = "staff", PasswordHash = "hashed", Role = UserRole.Staff, IsActive = true };
        _users.Setup(r => r.GetByUsernameAsync("staff")).ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify("hashed", "wrong")).Returns(false);

        await Assert.ThrowsAsync<AuthenticationException>(
            () => CreateService().LoginAsync(new LoginDto { Username = "staff", Password = "wrong" }));

        _tokenGenerator.Verify(g => g.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_UnknownUsername_ThrowsAuthenticationException()
    {
        _users.Setup(r => r.GetByUsernameAsync("ghost")).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<AuthenticationException>(
            () => CreateService().LoginAsync(new LoginDto { Username = "ghost", Password = "anything" }));
    }

    [Fact]
    public async Task LoginAsync_InactiveUser_ThrowsAuthenticationException()
    {
        var user = new User { Id = 1, Username = "disabled", PasswordHash = "hashed", Role = UserRole.Staff, IsActive = false };
        _users.Setup(r => r.GetByUsernameAsync("disabled")).ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        await Assert.ThrowsAsync<AuthenticationException>(
            () => CreateService().LoginAsync(new LoginDto { Username = "disabled", Password = "correct" }));
    }
}
