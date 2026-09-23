using Microsoft.AspNetCore.Mvc;
using OrderCompletion.Api.Ports;

namespace OrderCompletion.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderCompletionUseCase _useCase;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderCompletionUseCase useCase, ILogger<OrdersController> logger)
    {
        _useCase = useCase;
        _logger = logger;
    }

    [HttpPost(Name = "Complete")]
    public ActionResult Complete(List<int> orderIds)
    {
        _logger.LogDebug("Received completion request for {OrderCount} orders.", orderIds.Count);

        _useCase.CompleteOrders(orderIds);

        return Ok();
    }
}