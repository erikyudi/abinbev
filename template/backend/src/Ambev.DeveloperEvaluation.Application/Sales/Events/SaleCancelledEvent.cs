using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events;

public class SaleCancelledEvent : INotification
{
    public Guid SaleId { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime CancelledAt { get; set; } = DateTime.UtcNow;

    public SaleCancelledEvent(Guid saleId, string saleNumber)
    {
        SaleId = saleId;
        SaleNumber = saleNumber;
    }
}
