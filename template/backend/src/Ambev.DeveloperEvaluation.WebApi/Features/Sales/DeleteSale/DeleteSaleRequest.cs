namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.DeleteSale;

public class DeleteSaleRequest
{
    public Guid Id { get; set; }
}

public class DeleteSaleResponse
{
    public bool Success { get; set; }
}
