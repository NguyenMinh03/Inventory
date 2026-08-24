using FluentValidation;
using InventorySystem.Application.DTOs;

namespace InventorySystem.Application.Validators;

public class CreatePurchaseOrderDtoValidator : AbstractValidator<CreatePurchaseOrderDto>
{
    public CreatePurchaseOrderDtoValidator()
    {
        RuleFor(x => x.SupplierId).GreaterThan(0);
        RuleFor(x => x.Items).NotEmpty().WithMessage("A purchase order must have at least one item.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).GreaterThan(0);
            item.RuleFor(i => i.QuantityOrdered).GreaterThan(0);
            item.RuleFor(i => i.UnitCost).GreaterThanOrEqualTo(0);
        });
    }
}

public class ReceivePurchaseOrderDtoValidator : AbstractValidator<ReceivePurchaseOrderDto>
{
    public ReceivePurchaseOrderDtoValidator()
    {
        RuleFor(x => x.WarehouseId).GreaterThan(0);
        RuleFor(x => x.Items).NotEmpty().WithMessage("At least one item must be received.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.PurchaseOrderItemId).GreaterThan(0);
            item.RuleFor(i => i.QuantityReceived).GreaterThan(0);
        });
    }
}
