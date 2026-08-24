using InventorySystem.Application.DTOs;

namespace InventorySystem.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResultDto> LoginAsync(LoginDto dto);
}
