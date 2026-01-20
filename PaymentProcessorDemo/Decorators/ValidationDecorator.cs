namespace PaymentProcessorDemo.Decorators;

using Core;
using Models;

// Decorator 1: Validation
public class ValidationDecorator : PaymentProcessorDecorator
{
    public ValidationDecorator(IPaymentProcessor innerProcessor)
        : base(innerProcessor) { }

    public override PaymentResponse Process(PaymentRequest request)
    {
        Console.WriteLine($"[ValidationDecorator] Validating payment request...");

        // Kiểm tra idempotency key
        if (string.IsNullOrEmpty(request.IdempotencyKey))
        {
            var errorResponse = new PaymentResponse
            {
                Success = false,
                Message = "Idempotency key is required",
                Amount = request.Amount,
                ProcessedAt = DateTime.UtcNow,
            };
            Console.WriteLine(
                $"[ValidationDecorator] ✗ Validation failed: {errorResponse.Message}"
            );
            return errorResponse;
        }

        // Kiểm tra amount
        if (request.Amount <= 0)
        {
            var errorResponse = new PaymentResponse
            {
                Success = false,
                Message = "Amount must be greater than 0",
                Amount = request.Amount,
                ProcessedAt = DateTime.UtcNow,
            };
            Console.WriteLine(
                $"[ValidationDecorator] ✗ Validation failed: {errorResponse.Message}"
            );
            return errorResponse;
        }

        // Kiểm tra thẻ
        if (string.IsNullOrEmpty(request.CardNumber) || request.CardNumber.Length != 16)
        {
            var errorResponse = new PaymentResponse
            {
                Success = false,
                Message = "Invalid card number",
                Amount = request.Amount,
                ProcessedAt = DateTime.UtcNow,
            };
            Console.WriteLine(
                $"[ValidationDecorator] ✗ Validation failed: {errorResponse.Message}"
            );
            return errorResponse;
        }

        if (string.IsNullOrEmpty(request.CVC) || request.CVC.Length != 3)
        {
            var errorResponse = new PaymentResponse
            {
                Success = false,
                Message = "Invalid CVC",
                Amount = request.Amount,
                ProcessedAt = DateTime.UtcNow,
            };
            Console.WriteLine(
                $"[ValidationDecorator] ✗ Validation failed: {errorResponse.Message}"
            );
            return errorResponse;
        }

        Console.WriteLine($"[ValidationDecorator] ✓ All validations passed");

        return base.Process(request);
    }
}
