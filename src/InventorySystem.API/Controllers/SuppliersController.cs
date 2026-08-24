using FluentValidation;
using InventorySystem.Application.DTOs;
using InventorySystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventorySystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _supplierService;
    private readonly IValidator<CreateSupplierDto> _createValidator;
    private readonly IValidator<UpdateSupplierDto> _updateValidator;

    public SuppliersController(
        ISupplierService supplierService,
        IValidator<CreateSupplierDto> createValidator,
        IValidator<UpdateSupplierDto> updateValidator)
    {
        _supplierService = supplierService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SupplierDto>>> GetAll() =>
        Ok(await _supplierService.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SupplierDto>> GetById(int id)
    {
        var supplier = await _supplierService.GetByIdAsync(id);
        return supplier is null ? NotFound() : Ok(supplier);
    }

    [HttpPost]
    public async Task<ActionResult<SupplierDto>> Create(CreateSupplierDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);
        var supplier = await _supplierService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = supplier.Id }, supplier);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateSupplierDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);
        await _supplierService.UpdateAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Delete(int id)
    {
        await _supplierService.DeleteAsync(id);
        return NoContent();
    }
}
