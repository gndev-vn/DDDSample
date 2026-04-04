using Microsoft.AspNetCore.Authorization;
using PaymentAPI.Features.Payments.CompletePayment;
using PaymentAPI.Features.Payments.FailPayment;
using PaymentAPI.Features.Payments.GetPaymentById;
using PaymentAPI.Features.Payments.GetPaymentByOrderId;
using PaymentAPI.Features.Payments.GetPayments;
using Shared.Authentication;

namespace PaymentAPI.Features;

public static class PaymentEndpoints
{
    public static IEndpointRouteBuilder MapPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        var paymentReads = app.MapGroup("/api/Payments")
            .WithTags("Payments")
            .RequireAuthorization(new AuthorizeAttribute { Policy = Permissions.Payments.View });
        var paymentUpdates = app.MapGroup("/api/Payments")
            .WithTags("Payments")
            .RequireAuthorization(new AuthorizeAttribute { Policy = Permissions.Payments.Update });

        GetPaymentsEndpoint.Map(paymentReads);
        GetPaymentByIdEndpoint.Map(paymentReads);
        GetPaymentByOrderIdEndpoint.Map(paymentReads);
        CompletePaymentEndpoint.Map(paymentUpdates);
        FailPaymentEndpoint.Map(paymentUpdates);

        return app;
    }
}
