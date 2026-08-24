using FluentValidation;
using InventorySystem.Application.DTOs;
using InventorySystem.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InventorySystem.API.Controllers;

[ApiController]
[Route("api/stock")]
public class StockController : ControllerBase
{
    private readonly IStockService _stockService;
    private readonly IValidator<CreateStockMovementDto> _movementValidator;
    private readonly IValidator<CreateStockTransferDto> _transferValidator;

    public StockController(
        IStockService stockService,
        IValidator<CreateStockMovementDto> movementValidator,
        IValidator<CreateStockTransferDto> transferValidator)
    {
        _stockService = stockService;
        _movementValidator = movementValidator;
        _transferValidator = transferValidator;
    }

    [HttpGet("levels")]
    public async Task<ActionResult<IReadOnlyList<StockLevelDto>>> GetLevels() =>
        Ok(await _stockService.GetStockLevelsAsync());

    [HttpGet("levels/product/{productId:int}")]
    public async Task<ActionResult<IReadOnlyList<StockLevelDto>>> GetLevelsByProduct(int productId) =>
        Ok(await _stockService.GetStockLevelsByProductAsync(productId));

    [HttpGet("low")]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetLowStock() =>
        Ok(await _stockService.GetLowStockProductsAsync());

    [HttpPost("movements")]
    public async Task<ActionResult<StockMovementDto>> RecordMovement(CreateStockMovementDto dto)
    {
        await _movementValidator.ValidateAndThrowAsync(dto);
        var movement = await _stockService.RecordMovementAsync(dto);
        return Ok(movement);
    }

    [HttpPost("transfers")]
    public async Task<ActionResult<IReadOnlyList<StockMovementDto>>> Transfer(CreateStockTransferDto dto)
    {
        await _transferValidator.ValidateAndThrowAsync(dto);
        var movements = await _stockService.TransferAsync(dto);
        return Ok(movements);
    }
}
