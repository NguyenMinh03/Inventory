using InventorySystem.Application.DTOs;

namespace InventorySystem.Application.Interfaces;

public interface ISupplierService
{
    Task<IReadOnlyList<SupplierDto>> GetAllAsync();
    Task<SupplierDto?> GetByIdAsync(int id);
    Task<SupplierDto> CreateAsync(CreateSupplierDto dto);
    Task UpdateAsync(int id, UpdateSupplierDto dto);
    Task DeleteAsync(int id);
}
