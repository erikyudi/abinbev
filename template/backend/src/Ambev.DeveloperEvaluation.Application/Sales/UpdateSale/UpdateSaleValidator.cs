using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

public class UpdateSaleValidator : AbstractValidator<UpdateSaleCommand>
{
    public UpdateSaleValidator()
    {
        RuleFor(sale => sale.Id).NotEmpty();
        RuleFor(sale => sale.SaleNumber).NotEmpty().Length(1, 50);
        RuleFor(sale => sale.CustomerId).NotEmpty();
        RuleFor(sale => sale.BranchId).NotEmpty();
        RuleFor(sale => sale.Items).NotEmpty().WithMessage("At least one item is required.");
        
        RuleForEach(sale => sale.Items).ChildRules(items =>
        {
            items.RuleFor(item => item.ProductId).NotEmpty();
            items.RuleFor(item => item.Quantity).GreaterThan(0);
            items.RuleFor(item => item.UnitPrice).GreaterThan(0);
        });
    }
}
