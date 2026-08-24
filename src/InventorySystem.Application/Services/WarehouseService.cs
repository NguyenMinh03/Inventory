using AutoMapper;
using InventorySystem.Application.DTOs;
using InventorySystem.Application.Interfaces;
using InventorySystem.Domain.Entities;
using InventorySystem.Domain.Exceptions;
using InventorySystem.Domain.Interfaces;

namespace InventorySystem.Application.Services;

public class WarehouseService : IWarehouseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public WarehouseService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<WarehouseDto>> GetAllAsync()
    {
        var warehouses = await _unitOfWork.Warehouses.GetAllAsync();
        return _mapper.Map<List<WarehouseDto>>(warehouses);
    }

    public async Task<WarehouseDto?> GetByIdAsync(int id)
    {
        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(id);
        return warehouse is null ? null : _mapper.Map<WarehouseDto>(warehouse);
    }

    public async Task<WarehouseDto> CreateAsync(CreateWarehouseDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new DomainException("Warehouse name is required.");

        var warehouse = new Warehouse
        {
            Name = dto.Name,
            Address = dto.Address,
        };

        await _unitOfWork.Warehouses.AddAsync(warehouse);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<WarehouseDto>(warehouse);
    }

    public async Task UpdateAsync(int id, UpdateWarehouseDto dto)
    {
        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Warehouse {id} was not found.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new DomainException("Warehouse name is required.");

        warehouse.Name = dto.Name;
        warehouse.Address = dto.Address;
        warehouse.IsActive = dto.IsActive;

        _unitOfWork.Warehouses.Update(warehouse);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Warehouse {id} was not found.");

        _unitOfWork.Warehouses.Remove(warehouse);
        await _unitOfWork.SaveChangesAsync();
    }
}
