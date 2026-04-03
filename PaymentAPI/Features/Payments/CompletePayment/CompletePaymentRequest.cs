namespace PaymentAPI.Features.Payments.CompletePayment;

public class CompletePaymentRequest
{
    public string TransactionReference { get; set; } = string.Empty;
}