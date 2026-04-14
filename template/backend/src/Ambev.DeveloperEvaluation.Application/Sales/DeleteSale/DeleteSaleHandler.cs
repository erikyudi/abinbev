using Ambev.DeveloperEvaluation.Domain.Repositories;
using MassTransit;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;

public class DeleteSaleHandler : IRequestHandler<DeleteSaleCommand, DeleteSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMediator _mediator;
    private readonly IPublishEndpoint _publishEndpoint;

    public DeleteSaleHandler(ISaleRepository saleRepository, IMediator mediator, IPublishEndpoint publishEndpoint)
    {
        _saleRepository = saleRepository;
        _mediator = mediator;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<DeleteSaleResult> Handle(DeleteSaleCommand request, CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetByIdAsync(request.Id, cancellationToken);
        if (sale == null)
            throw new KeyNotFoundException($"Sale with id {request.Id} not found.");

        sale.Cancel();
        
        await _saleRepository.UpdateAsync(sale, cancellationToken);
        
        await _mediator.Publish(new Events.SaleCancelledEvent(sale.Id, sale.SaleNumber), cancellationToken);
        await _publishEndpoint.Publish(new Events.SaleCancelledEvent(sale.Id, sale.SaleNumber), cancellationToken);

        return new DeleteSaleResult { Success = true };
    }
}
