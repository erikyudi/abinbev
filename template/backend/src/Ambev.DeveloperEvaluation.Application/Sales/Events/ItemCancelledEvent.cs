using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events;

public class ItemCancelledEvent : INotification
{
    public Guid SaleId { get; set; }
    public Guid SaleItemId { get; set; }
    public DateTime CancelledAt { get; set; } = DateTime.UtcNow;

    public ItemCancelledEvent(Guid saleId, Guid saleItemId)
    {
        SaleId = saleId;
        SaleItemId = saleItemId;
    }
}
