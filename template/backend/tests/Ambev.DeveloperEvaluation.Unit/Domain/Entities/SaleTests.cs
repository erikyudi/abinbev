using Xunit;
using Ambev.DeveloperEvaluation.Domain.Entities;
using System;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

public class SaleTests
{
    [Fact]
    public void AddItem_Below4Quantity_NoDiscountApplied()
    {
        // Arrange
        var sale = new Sale();
        
        // Act (Quantity = 3, UnitPrice = 10 -> Total Expected = 30, no discount)
        sale.AddItem(Guid.NewGuid(), "Product A", 3, 10m);
        
        // Assert
        var item = Assert.Single(sale.Items);
        Assert.Equal(0, item.Discount);
        Assert.Equal(30m, item.TotalAmount);
        Assert.Equal(30m, sale.TotalSaleAmount);
    }

    [Fact]
    public void AddItem_Between4And9Quantity_10PercentDiscountApplied()
    {
        // Arrange
        var sale = new Sale();
        
        // Act (Quantity = 5, UnitPrice = 100 -> Total = 500, Discount = 50 -> Expected = 450)
        sale.AddItem(Guid.NewGuid(), "Product B", 5, 100m);
        
        // Assert
        var item = Assert.Single(sale.Items);
        Assert.Equal(50m, item.Discount);
        Assert.Equal(450m, item.TotalAmount);
        Assert.Equal(450m, sale.TotalSaleAmount);
    }

    [Fact]
    public void AddItem_Between10And20Quantity_20PercentDiscountApplied()
    {
        // Arrange
        var sale = new Sale();
        
        // Act (Quantity = 15, UnitPrice = 100 -> Total = 1500, Discount = 300 -> Expected = 1200)
        sale.AddItem(Guid.NewGuid(), "Product C", 15, 100m);
        
        // Assert
        var item = Assert.Single(sale.Items);
        Assert.Equal(300m, item.Discount);
        Assert.Equal(1200m, item.TotalAmount);
        Assert.Equal(1200m, sale.TotalSaleAmount);
    }

    [Fact]
    public void AddItem_UpdateExistingItem_RecalculatesProperly()
    {
        // Arrange
        var sale = new Sale();
        var productId = Guid.NewGuid();
        
        // First add 3 items (No discount)
        sale.AddItem(productId, "Product A", 3, 100m);
        // Add 2 more -> total = 5 -> should get 10% discount
        sale.AddItem(productId, "Product A", 2, 100m);
        
        // Assert
        var item = Assert.Single(sale.Items);
        Assert.Equal(5, item.Quantity);
        Assert.Equal(50m, item.Discount); // 500 * 0.10
        Assert.Equal(450m, item.TotalAmount);
    }
}
