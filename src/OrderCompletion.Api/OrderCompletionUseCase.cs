using Microsoft.Extensions.Logging.Abstractions;
using OrderCompletion.Api.Ports;

namespace OrderCompletion.Api;

public class OrderCompletionUseCase : IOrderCompletionUseCase
{
    private readonly IOrderCompletionRepository _orderCompletionRepository;
    private readonly INotificationClient _notificationClient;
    private readonly TimeProvider _clock;
    private readonly ILogger<OrderCompletionUseCase> _logger;

    public OrderCompletionUseCase(
        IOrderCompletionRepository orderCompletionRepository,
        INotificationClient notificationClient,
        TimeProvider? clock = null,
        ILogger<OrderCompletionUseCase>? logger = null)
    {
        _orderCompletionRepository = orderCompletionRepository;
        _notificationClient = notificationClient;
        _clock = clock ?? TimeProvider.System;
        _logger = logger ?? NullLogger<OrderCompletionUseCase>.Instance;
    }

    public void CompleteOrders(IReadOnlyCollection<int> orderIds)
    {
        foreach (var orderId in orderIds)
        {
            var order = _orderCompletionRepository.GetOrderById(orderId);
            if (order is null)
            {
                _logger.LogInformation("Order {OrderId} was not found.", orderId);

                continue;
            }

            if (!order.IsEligibleForCompletion(_clock.GetUtcNow().UtcDateTime))
            {
                _logger.LogDebug("Order {OrderId} is not eligible for completion.", orderId);

                continue;
            }

            try
            {
                _notificationClient.OrderCompleted(orderId);
            }
            catch (NotificationDeliveryException exception)
            {
                _logger.LogWarning(
                    exception,
                    "Completion notification for order {OrderId} could not be sent; skipping completion.", orderId);

                continue;
            }

            // Note: A database or I/O exception here could leave the order in the Submitted state after the notification service
            // has already processed the request. The scheduled job would then notify the same order again. The supplied notification
            // service provides no idempotency guarantee.
            _orderCompletionRepository.CompleteOrder(orderId);

            _logger.LogInformation("Order {OrderId} was completed", orderId);
        }
    }
}