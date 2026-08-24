using InventorySystem.Application.DTOs;

namespace InventorySystem.Application.Interfaces;

public interface IWarehouseService
{
    Task<IReadOnlyList<WarehouseDto>> GetAllAsync();
    Task<WarehouseDto?> GetByIdAsync(int id);
    Task<WarehouseDto> CreateAsync(CreateWarehouseDto dto);
    Task UpdateAsync(int id, UpdateWarehouseDto dto);
    Task DeleteAsync(int id);
}
