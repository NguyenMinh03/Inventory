using InventorySystem.Application.DTOs;
using InventorySystem.Application.Interfaces;
using InventorySystem.Domain.Exceptions;
using InventorySystem.Domain.Interfaces;

namespace InventorySystem.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public AuthService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, IJwtTokenGenerator tokenGenerator)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<AuthResultDto> LoginAsync(LoginDto dto)
    {
        var user = await _unitOfWork.Users.GetByUsernameAsync(dto.Username);

        // Same message whether the username doesn't exist or the password is
        // wrong - don't let a login response reveal which usernames are valid.
        if (user is null || !user.IsActive || !_passwordHasher.Verify(user.PasswordHash, dto.Password))
            throw new AuthenticationException("Invalid username or password.");

        var (token, expiresAtUtc) = _tokenGenerator.GenerateToken(user);

        return new AuthResultDto
        {
            Token = token,
            ExpiresAtUtc = expiresAtUtc,
            Username = user.Username,
            Role = user.Role.ToString(),
        };
    }
}
