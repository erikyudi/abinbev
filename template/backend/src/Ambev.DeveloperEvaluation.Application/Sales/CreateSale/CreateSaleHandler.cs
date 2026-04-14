using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Validation;
using AutoMapper;
using FluentValidation;
using MassTransit;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

public class CreateSaleHandler : IRequestHandler<CreateSaleCommand, CreateSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateSaleHandler(ISaleRepository saleRepository, IMapper mapper, IMediator mediator, IPublishEndpoint publishEndpoint)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
        _mediator = mediator;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<CreateSaleResult> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
    {
        var validator = new CreateSaleValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var sale = new Sale
        {
            Id = Guid.NewGuid(),
            SaleNumber = request.SaleNumber,
            SaleDate = request.SaleDate,
            CustomerId = request.CustomerId,
            CustomerName = request.CustomerName,
            BranchId = request.BranchId,
            BranchName = request.BranchName
        };

        foreach (var item in request.Items)
        {
            sale.AddItem(item.ProductId, item.ProductName, item.Quantity, item.UnitPrice);
        }

        // Validate the domain entity
        var domainValidator = new SaleValidator();
        var domainValidationResult = await domainValidator.ValidateAsync(sale, cancellationToken);
        if (!domainValidationResult.IsValid)
            throw new ValidationException(domainValidationResult.Errors);

        var createdSale = await _saleRepository.CreateAsync(sale, cancellationToken);

        await _mediator.Publish(new Events.SaleRegisteredEvent(createdSale.Id, createdSale.SaleNumber, createdSale.TotalSaleAmount), cancellationToken);
        await _publishEndpoint.Publish(new Events.SaleRegisteredEvent(createdSale.Id, createdSale.SaleNumber, createdSale.TotalSaleAmount), cancellationToken);

        var result = _mapper.Map<CreateSaleResult>(createdSale);
        return result;
    }
}
