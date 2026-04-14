using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.EventHandlers;

public class SaleEventHandlers :
    INotificationHandler<Events.SaleRegisteredEvent>,
    INotificationHandler<Events.SaleModifiedEvent>,
    INotificationHandler<Events.SaleCancelledEvent>,
    INotificationHandler<Events.ItemCancelledEvent>
{
    private readonly ILogger<SaleEventHandlers> _logger;

    public SaleEventHandlers(ILogger<SaleEventHandlers> logger)
    {
        _logger = logger;
    }

    public Task Handle(Events.SaleRegisteredEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Domain Event => SaleRegisteredEvent: Sale {SaleNumber} (Id: {SaleId}) registered with total {TotalAmount}.",
            notification.SaleNumber, notification.SaleId, notification.TotalAmount);
        return Task.CompletedTask;
    }

    public Task Handle(Events.SaleModifiedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Domain Event => SaleModifiedEvent: Sale {SaleNumber} (Id: {SaleId}) was modified.",
            notification.SaleNumber, notification.SaleId);
        return Task.CompletedTask;
    }

    public Task Handle(Events.SaleCancelledEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Domain Event => SaleCancelledEvent: Sale {SaleNumber} (Id: {SaleId}) was cancelled.",
            notification.SaleNumber, notification.SaleId);
        return Task.CompletedTask;
    }

    public Task Handle(Events.ItemCancelledEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Domain Event => ItemCancelledEvent: Item {ItemId} from Sale (Id: {SaleId}) was cancelled.",
            notification.SaleItemId, notification.SaleId);
        return Task.CompletedTask;
    }
}
