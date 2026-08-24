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
        RuleFor(x => x.Type).IsInEnum();

        RuleFor(x => x.RelatedWarehouseId)
            .NotNull()
            .WithMessage("A destination warehouse is required for a transfer.")
            .When(x => x.Type == MovementType.Transfer);

        RuleFor(x => x.RelatedWarehouseId)
            .NotEqual(x => x.WarehouseId)
            .WithMessage("Transfer source and destination warehouses must differ.")
            .When(x => x.Type == MovementType.Transfer && x.RelatedWarehouseId is not null);
    }
}
