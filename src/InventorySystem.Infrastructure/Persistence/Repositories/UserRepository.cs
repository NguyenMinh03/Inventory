using InventorySystem.Domain.Entities;
using InventorySystem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(int id) => await _context.Users.FindAsync(id);

    public async Task<User?> GetByUsernameAsync(string username) =>
        await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

    public async Task<IReadOnlyList<User>> GetAllAsync() => await _context.Users.ToListAsync();

    public async Task AddAsync(User entity) => await _context.Users.AddAsync(entity);

    public void Update(User entity) => _context.Users.Update(entity);

    public void Remove(User entity) => _context.Users.Remove(entity);
}
