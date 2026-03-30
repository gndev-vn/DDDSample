using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderingAPI.Features.Orders.Commands;
using OrderingAPI.Features.Orders.Models;
using OrderingAPI.Features.Orders.Queries;
using Shared.Models;

namespace OrderingAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<OrderModel>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var orders = await mediator.Send(new GetOrdersQuery());
        return Ok(ApiResponse.Success(orders, "Orders retrieved successfully"));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<OrderModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var order = await mediator.Send(new GetOrderByIdQuery(id));

        if (order == null)
        {
            return NotFound(ApiResponse.Error("Order not found"));
        }

        return Ok(ApiResponse.Success(order, "Order retrieved successfully"));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<OrderModel>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateOrderModel request)
    {
        var result = await mediator.Send(new CreateOrderCommand(
            request.CustomerId,
            request.Lines,
            request.ShippingAddress,
            request.BillingAddress
        ));

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse.Success(result, "Order created successfully"));
    }

    [HttpPost("{id:guid}/pay")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Pay(Guid id)
    {
        var result = await mediator.Send(new PayOrderCommand(id));
        return Ok(ApiResponse.Success(result, "Order paid successfully"));
    }

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var result = await mediator.Send(new CancelOrderCommand(id));
        return Ok(ApiResponse.Success(result, "Order cancelled successfully"));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await mediator.Send(new DeleteOrderCommand(id));
        return Ok(ApiResponse.Success("Order deleted successfully"));
    }
}
