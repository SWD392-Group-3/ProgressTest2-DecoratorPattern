namespace PaymentProcessorDemo.Models;

public class PaymentRequest
{
    public string IdempotencyKey { get; set; }
    public decimal Amount { get; set; }
    public string CardNumber { get; set; }
    public string CardholderName { get; set; }
    public string ExpirationDate { get; set; }
    public string CVC { get; set; }
}
