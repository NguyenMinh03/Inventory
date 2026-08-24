using FluentValidation;
using InventorySystem.Application.DTOs;
using InventorySystem.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InventorySystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PurchaseOrdersController : ControllerBase
{
    private readonly IPurchaseOrderService _purchaseOrderService;
    private readonly IValidator<CreatePurchaseOrderDto> _createValidator;
    private readonly IValidator<ReceivePurchaseOrderDto> _receiveValidator;

    public PurchaseOrdersController(
        IPurchaseOrderService purchaseOrderService,
        IValidator<CreatePurchaseOrderDto> createValidator,
        IValidator<ReceivePurchaseOrderDto> receiveValidator)
    {
        _purchaseOrderService = purchaseOrderService;
        _createValidator = createValidator;
        _receiveValidator = receiveValidator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PurchaseOrderDto>>> GetAll() =>
        Ok(await _purchaseOrderService.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PurchaseOrderDto>> GetById(int id)
    {
        var order = await _purchaseOrderService.GetByIdAsync(id);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<PurchaseOrderDto>> Create(CreatePurchaseOrderDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);
        var order = await _purchaseOrderService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpPost("{id:int}/send")]
    public async Task<IActionResult> Send(int id)
    {
        await _purchaseOrderService.SendAsync(id);
        return NoContent();
    }

    [HttpPost("{id:int}/receive")]
    public async Task<IActionResult> Receive(int id, ReceivePurchaseOrderDto dto)
    {
        await _receiveValidator.ValidateAndThrowAsync(dto);
        await _purchaseOrderService.ReceiveAsync(id, dto);
        return NoContent();
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        await _purchaseOrderService.CancelAsync(id);
        return NoContent();
    }
}
