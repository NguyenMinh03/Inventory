using InventorySystem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Infrastructure.Persistence.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly DbSet<T> _dbSet;

    public Repository(AppDbContext context)
    {
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

    public async Task<IReadOnlyList<T>> GetAllAsync() => await _dbSet.ToListAsync();

    public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

    public void Update(T entity) => _dbSet.Update(entity);

    public void Remove(T entity) => _dbSet.Remove(entity);
}
