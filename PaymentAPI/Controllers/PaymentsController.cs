using Mediator;
using Microsoft.AspNetCore.Mvc;
using PaymentAPI.Features.Payments.Commands;
using PaymentAPI.Features.Payments.Models;
using PaymentAPI.Features.Payments.Queries;
using Shared.Models;

namespace PaymentAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var payments = await mediator.Send(new GetPaymentsQuery());
        return Ok(ApiResponse.Success(payments, "Payments retrieved successfully"));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var payment = await mediator.Send(new GetPaymentByIdQuery(id));
        if (payment == null)
        {
            return NotFound(ApiResponse.Error("Payment not found"));
        }

        return Ok(ApiResponse.Success(payment, "Payment retrieved successfully"));
    }

    [HttpGet("orders/{orderId:guid}")]
    public async Task<IActionResult> GetByOrderId(Guid orderId)
    {
        var payment = await mediator.Send(new GetPaymentByOrderIdQuery(orderId));
        if (payment == null)
        {
            return NotFound(ApiResponse.Error("Payment not found"));
        }

        return Ok(ApiResponse.Success(payment, "Payment retrieved successfully"));
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id, [FromBody] CompletePaymentRequest request)
    {
        var payment = await mediator.Send(new CompletePaymentCommand(id, request.TransactionReference));
        return Ok(ApiResponse.Success(payment, "Payment completed successfully"));
    }

    [HttpPost("{id:guid}/fail")]
    public async Task<IActionResult> Fail(Guid id, [FromBody] FailPaymentRequest request)
    {
        var payment = await mediator.Send(new FailPaymentCommand(id, request.Reason));
        return Ok(ApiResponse.Success(payment, "Payment marked as failed"));
    }
}
