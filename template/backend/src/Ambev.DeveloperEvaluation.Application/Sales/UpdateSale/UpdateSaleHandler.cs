using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Validation;
using AutoMapper;
using FluentValidation;
using MassTransit;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

public class UpdateSaleHandler : IRequestHandler<UpdateSaleCommand, UpdateSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly IPublishEndpoint _publishEndpoint;

    public UpdateSaleHandler(ISaleRepository saleRepository, IMapper mapper, IMediator mediator, IPublishEndpoint publishEndpoint)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
        _mediator = mediator;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<UpdateSaleResult> Handle(UpdateSaleCommand request, CancellationToken cancellationToken)
    {
        var validator = new UpdateSaleValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var sale = await _saleRepository.GetByIdAsync(request.Id, cancellationToken);
        if (sale == null)
            throw new KeyNotFoundException($"Sale with id {request.Id} not found.");

        sale.SaleNumber = request.SaleNumber;
        sale.SaleDate = request.SaleDate;
        sale.CustomerId = request.CustomerId;
        sale.CustomerName = request.CustomerName;
        sale.BranchId = request.BranchId;
        sale.BranchName = request.BranchName;

        var items = request.Items.Select(i => new SaleItem
        {
            SaleId = sale.Id,
            ProductId = i.ProductId,
            ProductName = i.ProductName,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice
        });

        sale.UpdateItems(items);

        var domainValidator = new SaleValidator();
        var domainValidationResult = await domainValidator.ValidateAsync(sale, cancellationToken);
        if (!domainValidationResult.IsValid)
            throw new ValidationException(domainValidationResult.Errors);

        var updatedSale = await _saleRepository.UpdateAsync(sale, cancellationToken);

        await _mediator.Publish(new Events.SaleModifiedEvent(updatedSale.Id, updatedSale.SaleNumber), cancellationToken);
        await _publishEndpoint.Publish(new Events.SaleModifiedEvent(updatedSale.Id, updatedSale.SaleNumber), cancellationToken);

        return _mapper.Map<UpdateSaleResult>(updatedSale);
    }
}
