using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events;

public class SaleModifiedEvent : INotification
{
    public Guid SaleId { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    public SaleModifiedEvent(Guid saleId, string saleNumber)
    {
        SaleId = saleId;
        SaleNumber = saleNumber;
    }
}
