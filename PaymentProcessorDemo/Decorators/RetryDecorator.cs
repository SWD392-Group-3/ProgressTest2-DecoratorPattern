namespace PaymentProcessorDemo.Decorators;

using Core;
using Models;

// Decorator 3: Retry
public class RetryDecorator : PaymentProcessorDecorator
{
    private readonly int _maxRetries;
    private readonly int _delayMs;

    public RetryDecorator(IPaymentProcessor innerProcessor, int maxRetries = 3, int delayMs = 500)
        : base(innerProcessor)
    {
        _maxRetries = maxRetries;
        _delayMs = delayMs;
    }

    public override PaymentResponse Process(PaymentRequest request)
    {
        Console.WriteLine($"[RetryDecorator] Starting retry logic (max retries: {_maxRetries})");

        int retryCount = 0;

        while (true)
        {
            try
            {
                var response = base.Process(request);

                if (retryCount > 0)
                {
                    Console.WriteLine($"[RetryDecorator] ✓ Succeeded after {retryCount} retries");
                }

                return response;
            }
            catch (TimeoutException ex)
            {
                retryCount++;
                Console.WriteLine($"[RetryDecorator] ⚠ Attempt {retryCount} failed: {ex.Message}");

                if (retryCount >= _maxRetries)
                {
                    Console.WriteLine($"[RetryDecorator] ✗ Max retries ({_maxRetries}) exceeded");

                    return new PaymentResponse
                    {
                        Success = false,
                        Message = $"Payment failed after {_maxRetries} retries: {ex.Message}",
                        Amount = request.Amount,
                        ProcessedAt = DateTime.UtcNow,
                    };
                }

                Console.WriteLine($"[RetryDecorator] Waiting {_delayMs}ms before retry...");
                Thread.Sleep(_delayMs);
            }
        }
    }
}
