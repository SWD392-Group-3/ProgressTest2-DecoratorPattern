namespace PaymentProcessorDemo.Decorators;

using Core;
using Models;

// Decorator 5: Metrics
public class MetricsDecorator : PaymentProcessorDecorator
{
    private int _totalCalls = 0;
    private int _successCount = 0;
    private int _failureCount = 0;
    private long _totalDurationMs = 0;

    public MetricsDecorator(IPaymentProcessor innerProcessor)
        : base(innerProcessor) { }

    public override PaymentResponse Process(PaymentRequest request)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            var response = base.Process(request);

            var duration = DateTime.UtcNow - startTime;
            _totalCalls++;
            _totalDurationMs += (long)duration.TotalMilliseconds;

            if (response.Success)
                _successCount++;
            else
                _failureCount++;

            Console.WriteLine(
                $"[MetricsDecorator] Call #{_totalCalls} completed in {duration.TotalMilliseconds:F2}ms"
            );

            return response;
        }
        catch
        {
            _totalCalls++;
            _failureCount++;
            throw;
        }
    }

    public void PrintMetrics()
    {
        Console.WriteLine("\n" + new string('=', 60));
        Console.WriteLine("📊 PAYMENT PROCESSOR METRICS");
        Console.WriteLine(new string('=', 60));
        Console.WriteLine($"Total Calls:           {_totalCalls}");
        Console.WriteLine(
            $"Successful:            {_successCount} ({(_totalCalls > 0 ? (_successCount * 100 / _totalCalls) : 0)}%)"
        );
        Console.WriteLine(
            $"Failed:                {_failureCount} ({(_totalCalls > 0 ? (_failureCount * 100 / _totalCalls) : 0)}%)"
        );
        Console.WriteLine(
            $"Average Duration:      {(_totalCalls > 0 ? (_totalDurationMs / _totalCalls) : 0)}ms"
        );
        Console.WriteLine($"Total Duration:        {_totalDurationMs}ms");
        Console.WriteLine(new string('=', 60) + "\n");
    }
}
