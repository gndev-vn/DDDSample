namespace PaymentAPI.Features.Payments.Models;

public class CompletePaymentRequest
{
    public string TransactionReference { get; set; } = string.Empty;
}
