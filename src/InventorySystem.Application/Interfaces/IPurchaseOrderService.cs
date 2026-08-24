using InventorySystem.Application.DTOs;

namespace InventorySystem.Application.Interfaces;

public interface IPurchaseOrderService
{
    Task<IReadOnlyList<PurchaseOrderDto>> GetAllAsync();
    Task<PurchaseOrderDto?> GetByIdAsync(int id);
    Task<PurchaseOrderDto> CreateAsync(CreatePurchaseOrderDto dto);
    Task SendAsync(int id);
    Task ReceiveAsync(int id, ReceivePurchaseOrderDto dto);
    Task CancelAsync(int id);
}
