namespace PaymentAPI.Features.Payments.FailPayment;

public class FailPaymentRequest
{
    public string Reason { get; set; } = string.Empty;
}