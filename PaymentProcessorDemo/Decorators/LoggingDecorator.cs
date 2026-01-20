namespace PaymentProcessorDemo.Decorators;

using Core;
using Models;

// Decorator 4: Logging
public class LoggingDecorator : PaymentProcessorDecorator
{
    public LoggingDecorator(IPaymentProcessor innerProcessor)
        : base(innerProcessor) { }

    public override PaymentResponse Process(PaymentRequest request)
    {
        var startTime = DateTime.UtcNow;
        Console.WriteLine($"[LoggingDecorator] ▶ START processing payment");
        Console.WriteLine($"[LoggingDecorator]   Amount: ${request.Amount}");
        Console.WriteLine($"[LoggingDecorator]   Card: {MaskCardNumber(request.CardNumber)}");
        Console.WriteLine($"[LoggingDecorator]   Timestamp: {startTime:yyyy-MM-dd HH:mm:ss.fff}");

        try
        {
            var response = base.Process(request);

            var duration = DateTime.UtcNow - startTime;
            Console.WriteLine($"[LoggingDecorator] ◀ END processing payment");
            Console.WriteLine($"[LoggingDecorator]   Success: {response.Success}");
            Console.WriteLine($"[LoggingDecorator]   Message: {response.Message}");
            Console.WriteLine($"[LoggingDecorator]   Duration: {duration.TotalMilliseconds:F2}ms");

            return response;
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            Console.WriteLine($"[LoggingDecorator] ◀ ERROR during payment processing");
            Console.WriteLine($"[LoggingDecorator]   Exception: {ex.GetType().Name}: {ex.Message}");
            Console.WriteLine($"[LoggingDecorator]   Duration: {duration.TotalMilliseconds:F2}ms");

            throw;
        }
    }

    private string MaskCardNumber(string cardNumber)
    {
        if (cardNumber.Length < 8)
            return "****";

        return $"****-****-****-{cardNumber.Substring(cardNumber.Length - 4)}";
    }
}
