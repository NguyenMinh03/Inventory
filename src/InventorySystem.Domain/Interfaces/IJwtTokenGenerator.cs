using InventorySystem.Domain.Entities;

namespace InventorySystem.Domain.Interfaces;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAtUtc) GenerateToken(User user);
}
