using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events;

public class SaleRegisteredEvent : INotification
{
    public Guid SaleId { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    public SaleRegisteredEvent(Guid saleId, string saleNumber, decimal totalAmount)
    {
        SaleId = saleId;
        SaleNumber = saleNumber;
        TotalAmount = totalAmount;
    }
}
