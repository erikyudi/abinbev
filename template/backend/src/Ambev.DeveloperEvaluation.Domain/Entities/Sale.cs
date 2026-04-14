using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Represents a Sale transaction.
/// </summary>
public class Sale : BaseEntity
{
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; } = DateTime.UtcNow;

    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;

    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;

    public decimal TotalSaleAmount { get; private set; }
    public bool IsCancelled { get; private set; } = false;

    private readonly List<SaleItem> _items = new();
    public IReadOnlyCollection<SaleItem> Items => _items.AsReadOnly();

    public Sale()
    {
    }

    /// <summary>
    /// Adds an item to the sale, applying discount rules and recalculating totals.
    /// </summary>
    public void AddItem(Guid productId, string productName, int quantity, decimal unitPrice)
    {
        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId && !i.IsCancelled);
        if (existingItem != null)
        {
            // Just update the quantity and recalculate
            existingItem.Quantity += quantity;
            ApplyDiscountAndCalculateTotal(existingItem);
        }
        else
        {
            var newItem = new SaleItem
            {
                Id = Guid.NewGuid(),
                SaleId = this.Id,
                ProductId = productId,
                ProductName = productName,
                Quantity = quantity,
                UnitPrice = unitPrice
            };

            ApplyDiscountAndCalculateTotal(newItem);
            _items.Add(newItem);
        }

        RecalculateTotalSaleAmount();
    }

    /// <summary>
    /// Discards and replaces current items with the provided ones. Useful for Updates.
    /// </summary>
    public void UpdateItems(IEnumerable<SaleItem> newItems)
    {
        _items.Clear();
        foreach (var item in newItems)
        {
            ApplyDiscountAndCalculateTotal(item);
            _items.Add(item);
        }
        RecalculateTotalSaleAmount();
    }

    /// <summary>
    /// Cancels the sale and all its active items.
    /// </summary>
    public void Cancel()
    {
        IsCancelled = true;
        foreach (var item in _items.Where(i => !i.IsCancelled))
        {
            item.Cancel();
        }
    }

    /// <summary>
    /// Cancels a specific item in the sale.
    /// </summary>
    public void CancelItem(Guid saleItemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == saleItemId);
        if (item != null && !item.IsCancelled)
        {
            item.Cancel();
            RecalculateTotalSaleAmount();
        }
    }

    private void ApplyDiscountAndCalculateTotal(SaleItem item)
    {
        item.Discount = 0;
        
        // Business Rules for Discount
        // Purchases >= 4 and < 10 items have a 10% discount
        // Purchases >= 10 and <= 20 items have a 20% discount
        // Purchases < 4 items cannot have a discount
        
        if (item.Quantity >= 10 && item.Quantity <= 20)
        {
            item.Discount = item.UnitPrice * item.Quantity * 0.20m;
        }
        else if (item.Quantity >= 4 && item.Quantity < 10)
        {
            item.Discount = item.UnitPrice * item.Quantity * 0.10m;
        }

        item.TotalAmount = (item.UnitPrice * item.Quantity) - item.Discount;
    }

    private void RecalculateTotalSaleAmount()
    {
        TotalSaleAmount = _items.Where(i => !i.IsCancelled).Sum(i => i.TotalAmount);
    }
}
