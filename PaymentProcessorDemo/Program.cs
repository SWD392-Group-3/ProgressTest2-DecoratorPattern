using PaymentProcessorDemo.Core;
using PaymentProcessorDemo.Decorators;
using PaymentProcessorDemo.Models;

Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
Console.WriteLine("║   PAYMENT PROCESSOR - DECORATOR DESIGN PATTERN DEMO       ║");
Console.WriteLine("╚═══════════════════════════════════════════════════════════╝\n");

// Tạo concrete component
IPaymentProcessor processor = new StripeLikePaymentProcessor();

// Bọc với các decorators từ trong ra ngoài
processor = new MetricsDecorator(processor);
processor = new LoggingDecorator(processor);
processor = new RetryDecorator(processor, maxRetries: 3, delayMs: 300);
processor = new IdempotencyDecorator(processor);
processor = new ValidationDecorator(processor);

var metricsDecorator = processor as MetricsDecorator;
if (metricsDecorator == null)
{
    // Tìm MetricsDecorator trong stack
    var current = processor as PaymentProcessorDecorator;
    while (current != null && metricsDecorator == null)
    {
        metricsDecorator = current as MetricsDecorator;
        current = current._innerProcessor as PaymentProcessorDecorator;
    }
}

// Test Case 1: Thanh toán hợp lệ
Console.WriteLine("\n" + new string('─', 60));
Console.WriteLine("TEST 1: Valid Payment");
Console.WriteLine(new string('─', 60));

var validPayment = new PaymentRequest
{
    IdempotencyKey = "txn_001_valid",
    Amount = 99.99m,
    CardNumber = "4532015112830366",
    CardholderName = "John Doe",
    ExpirationDate = "12/25",
    CVC = "123",
};

var response1 = processor.Process(validPayment);
Console.WriteLine(
    $"\n→ Result: {(response1.Success ? "✓ SUCCESS" : "✗ FAILED")} - {response1.Message}\n"
);

// Test Case 2: Cùng idempotency key (phải trả về cached result)
Console.WriteLine(new string('─', 60));
Console.WriteLine("TEST 2: Duplicate Request (Same Idempotency Key)");
Console.WriteLine(new string('─', 60));

var duplicatePayment = new PaymentRequest
{
    IdempotencyKey = "txn_001_valid", // Cùng key
    Amount = 99.99m,
    CardNumber = "4532015112830366",
    CardholderName = "John Doe",
    ExpirationDate = "12/25",
    CVC = "123",
};

var response2 = processor.Process(duplicatePayment);
Console.WriteLine(
    $"\n→ Result: {(response2.Success ? "✓ SUCCESS" : "✗ FAILED")} - {response2.Message}\n"
);

// Test Case 3: Thanh toán không hợp lệ (amount <= 0)
Console.WriteLine(new string('─', 60));
Console.WriteLine("TEST 3: Invalid Payment (Amount <= 0)");
Console.WriteLine(new string('─', 60));

var invalidPayment = new PaymentRequest
{
    IdempotencyKey = "txn_002_invalid",
    Amount = -50m,
    CardNumber = "4532015112830366",
    CardholderName = "Jane Doe",
    ExpirationDate = "12/25",
    CVC = "456",
};

var response3 = processor.Process(invalidPayment);
Console.WriteLine(
    $"\n→ Result: {(response3.Success ? "✓ SUCCESS" : "✗ FAILED")} - {response3.Message}\n"
);

// Test Case 4: Thẻ không hợp lệ
Console.WriteLine(new string('─', 60));
Console.WriteLine("TEST 4: Invalid Payment (Bad Card Number)");
Console.WriteLine(new string('─', 60));

var badCardPayment = new PaymentRequest
{
    IdempotencyKey = "txn_003_badcard",
    Amount = 50m,
    CardNumber = "1234", // Quá ngắn
    CardholderName = "Bob Smith",
    ExpirationDate = "12/25",
    CVC = "789",
};

var response4 = processor.Process(badCardPayment);
Console.WriteLine(
    $"\n→ Result: {(response4.Success ? "✓ SUCCESS" : "✗ FAILED")} - {response4.Message}\n"
);

// Test Case 5: Thanh toán hợp lệ khác
Console.WriteLine(new string('─', 60));
Console.WriteLine("TEST 5: Another Valid Payment");
Console.WriteLine(new string('─', 60));

var anotherPayment = new PaymentRequest
{
    IdempotencyKey = "txn_005_another",
    Amount = 149.50m,
    CardNumber = "5425233010103442",
    CardholderName = "Alice Johnson",
    ExpirationDate = "06/26",
    CVC = "999",
};

var response5 = processor.Process(anotherPayment);
Console.WriteLine(
    $"\n→ Result: {(response5.Success ? "✓ SUCCESS" : "✗ FAILED")} - {response5.Message}\n"
);

// In ra metrics
metricsDecorator?.PrintMetrics();

Console.WriteLine("\n" + new string('═', 60));
Console.WriteLine("✓ Demo completed!");
Console.WriteLine(new string('═', 60));
