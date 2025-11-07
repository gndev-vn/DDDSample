using Mediator;
using Microsoft.AspNetCore.Mvc;
using OrderingAPI.Features.Order.Commands;
using OrderingAPI.Features.Order.Models;
using OrderingAPI.Features.Order.Queries;

namespace OrderingAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var orders = await mediator.Send(new GetOrdersQuery());
        return Ok(orders);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var order = await mediator.Send(new GetOrderByIdQuery(id));

        if (order == null)
        {
            return NotFound();
        }

        return Ok(order);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderModel request)
    {
        var result = await mediator.Send(new CreateOrderCommand(
            request.CustomerId,
            request.Lines,
            request.ShippingAddress,
            request.BillingAddress
        ));

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPost("{id:guid}/pay")]
    public async Task<IActionResult> Pay(Guid id)
    {
        var result = await mediator.Send(new PayOrderCommand(id));
        return Ok(result);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var result = await mediator.Send(new CancelOrderCommand(id));
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await mediator.Send(new DeleteOrderCommand(id));
        return NoContent();
    }
}