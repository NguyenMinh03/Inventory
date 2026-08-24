using FluentValidation;
using InventorySystem.Application.DTOs;
using InventorySystem.Domain.Enums;

namespace InventorySystem.Application.Validators;

public class CreateStockMovementDtoValidator : AbstractValidator<CreateStockMovementDto>
{
    public CreateStockMovementDtoValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.WarehouseId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Type)
            .IsInEnum()
            .NotEqual(MovementType.Transfer)
            .WithMessage("Use POST /api/stock/transfers to move stock between warehouses.");
    }
}

public class CreateStockTransferDtoValidator : AbstractValidator<CreateStockTransferDto>
{
    public CreateStockTransferDtoValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.SourceWarehouseId).GreaterThan(0);
        RuleFor(x => x.DestinationWarehouseId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);

        RuleFor(x => x.DestinationWarehouseId)
            .NotEqual(x => x.SourceWarehouseId)
            .WithMessage("Source and destination warehouses must differ.");
    }
}
