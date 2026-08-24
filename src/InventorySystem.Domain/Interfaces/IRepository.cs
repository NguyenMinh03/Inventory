namespace InventorySystem.Domain.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);

    // includeProperties are dotted navigation paths (e.g. "Items.Product") to
    // eager-load. Kept as plain strings, not an Expression/IQueryable-based
    // API, so this interface doesn't force a persistence-technology reference
    // onto whatever layer calls it.
    Task<T?> GetByIdAsync(int id, params string[] includeProperties);

    Task<IReadOnlyList<T>> GetAllAsync();
    Task<IReadOnlyList<T>> GetAllAsync(params string[] includeProperties);

    Task AddAsync(T entity);
    void Update(T entity);
    void Remove(T entity);
}
