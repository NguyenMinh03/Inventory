using InventorySystem.Domain.Entities;

namespace InventorySystem.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUsernameAsync(string username);
    Task<IReadOnlyList<User>> GetAllAsync();
    Task AddAsync(User entity);
    void Update(User entity);
    void Remove(User entity);
}
