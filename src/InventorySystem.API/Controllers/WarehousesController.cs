using FluentValidation;
using InventorySystem.Application.DTOs;
using InventorySystem.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InventorySystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WarehousesController : ControllerBase
{
    private readonly IWarehouseService _warehouseService;
    private readonly IValidator<CreateWarehouseDto> _createValidator;
    private readonly IValidator<UpdateWarehouseDto> _updateValidator;

    public WarehousesController(
        IWarehouseService warehouseService,
        IValidator<CreateWarehouseDto> createValidator,
        IValidator<UpdateWarehouseDto> updateValidator)
    {
        _warehouseService = warehouseService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<WarehouseDto>>> GetAll() =>
        Ok(await _warehouseService.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<WarehouseDto>> GetById(int id)
    {
        var warehouse = await _warehouseService.GetByIdAsync(id);
        return warehouse is null ? NotFound() : Ok(warehouse);
    }

    [HttpPost]
    public async Task<ActionResult<WarehouseDto>> Create(CreateWarehouseDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);
        var warehouse = await _warehouseService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = warehouse.Id }, warehouse);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateWarehouseDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);
        await _warehouseService.UpdateAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _warehouseService.DeleteAsync(id);
        return NoContent();
    }
}
