namespace PaymentProcessorDemo.Decorators;

using Core;
using Models;

// Decorator 2: Idempotency
public class IdempotencyDecorator : PaymentProcessorDecorator
{
    private readonly Dictionary<string, PaymentResponse> _cache = new();

    public IdempotencyDecorator(IPaymentProcessor innerProcessor)
        : base(innerProcessor) { }

    public override PaymentResponse Process(PaymentRequest request)
    {
        Console.WriteLine(
            $"[IdempotencyDecorator] Checking idempotency key: {request.IdempotencyKey}"
        );

        // Nếu key đã tồn tại trong cache, trả lại kết quả cũ
        if (_cache.ContainsKey(request.IdempotencyKey))
        {
            var cachedResponse = _cache[request.IdempotencyKey];
            Console.WriteLine(
                $"[IdempotencyDecorator] ✓ Found cached result: {cachedResponse.TransactionId}"
            );
            return cachedResponse;
        }

        // Xử lý lần đầu tiên
        Console.WriteLine($"[IdempotencyDecorator] New request, processing...");
        var response = base.Process(request);

        // Lưu vào cache nếu thành công
        if (response.Success)
        {
            _cache[request.IdempotencyKey] = response;
            Console.WriteLine(
                $"[IdempotencyDecorator] ✓ Cached result for key: {request.IdempotencyKey}"
            );
        }

        return response;
    }
}
