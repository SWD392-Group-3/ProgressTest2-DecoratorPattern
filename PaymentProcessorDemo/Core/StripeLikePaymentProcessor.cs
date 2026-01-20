namespace PaymentProcessorDemo.Core;

using Models;

// Concrete Component - Giả lập cổng thanh toán Stripe
public class StripeLikePaymentProcessor : IPaymentProcessor
{
    private static readonly Random _random = new Random();

    public PaymentResponse Process(PaymentRequest request)
    {
        Console.WriteLine(
            $"[StripeLikePaymentProcessor] Processing payment of ${request.Amount}..."
        );

        // Giả lập lỗi tạm thời (5% xác suất)
        if (_random.Next(100) < 5)
        {
            throw new TimeoutException("Stripe gateway timeout (simulated temporary error)");
        }

        // Giả lập thành công
        var transactionId = $"txn_{Guid.NewGuid().ToString().Substring(0, 8)}";

        var response = new PaymentResponse
        {
            Success = true,
            TransactionId = transactionId,
            Amount = request.Amount,
            Message = "Payment processed successfully",
            ProcessedAt = DateTime.UtcNow,
        };

        Console.WriteLine($"[StripeLikePaymentProcessor] ✓ Transaction ID: {transactionId}");

        return response;
    }
}
