# Implementation Guidelines - How to Extend the Decorator Pattern

## 🎯 How to Add a New Decorator

### Step 1: Understand the Pattern

Every decorator:

- ✓ Extends `PaymentProcessorDecorator`
- ✓ Has a constructor accepting `IPaymentProcessor`
- ✓ Overrides `Process(PaymentRequest request)` method
- ✓ Can add behavior BEFORE and/or AFTER `base.Process(request)`
- ✓ Returns `PaymentResponse`

### Step 2: Create Your Decorator

#### Example: RateLimitDecorator

```csharp
namespace PaymentProcessorDemo.Decorators;

using Models;
using Core;

// Decorator: Rate Limiting
public class RateLimitDecorator : PaymentProcessorDecorator
{
    private readonly int _maxRequestsPerMinute;
    private readonly Dictionary<string, List<DateTime>> _requestLog = new();

    public RateLimitDecorator(
        IPaymentProcessor innerProcessor,
        int maxRequestsPerMinute = 10)
        : base(innerProcessor)
    {
        _maxRequestsPerMinute = maxRequestsPerMinute;
    }

    public override PaymentResponse Process(PaymentRequest request)
    {
        Console.WriteLine($"[RateLimitDecorator] Checking rate limit...");

        var cardNumber = request.CardNumber;

        // Initialize request log for this card if needed
        if (!_requestLog.ContainsKey(cardNumber))
        {
            _requestLog[cardNumber] = new List<DateTime>();
        }

        // Remove old requests (older than 1 minute)
        var now = DateTime.UtcNow;
        _requestLog[cardNumber].RemoveAll(time =>
            (now - time).TotalMinutes > 1);

        // Check if rate limit exceeded
        if (_requestLog[cardNumber].Count >= _maxRequestsPerMinute)
        {
            var errorResponse = new PaymentResponse
            {
                Success = false,
                Message = $"Rate limit exceeded. Max {_maxRequestsPerMinute} requests per minute",
                Amount = request.Amount,
                ProcessedAt = DateTime.UtcNow
            };

            Console.WriteLine($"[RateLimitDecorator] ✗ Rate limit exceeded");
            return errorResponse;
        }

        // Record this request
        _requestLog[cardNumber].Add(now);
        Console.WriteLine($"[RateLimitDecorator] ✓ Request #{_requestLog[cardNumber].Count} allowed");

        // Proceed to next decorator/component
        return base.Process(request);
    }
}
```

#### Example: EncryptionDecorator

```csharp
namespace PaymentProcessorDemo.Decorators;

using Models;
using Core;
using System.Text;

// Decorator: Encryption (simulate)
public class EncryptionDecorator : PaymentProcessorDecorator
{
    public EncryptionDecorator(IPaymentProcessor innerProcessor)
        : base(innerProcessor)
    {
    }

    public override PaymentResponse Process(PaymentRequest request)
    {
        Console.WriteLine($"[EncryptionDecorator] Encrypting sensitive data...");

        // Simulate encryption
        var encryptedCard = EncryptCardNumber(request.CardNumber);
        Console.WriteLine($"[EncryptionDecorator] ✓ Card encrypted: {encryptedCard}");

        // Process with encrypted data (in real world, would use actual encryption)
        var response = base.Process(request);

        // Optionally decrypt response (in real world)
        Console.WriteLine($"[EncryptionDecorator] ✓ Response processed with encryption");

        return response;
    }

    private string EncryptCardNumber(string cardNumber)
    {
        // Simplified: just show first 4 and last 4
        return $"{cardNumber.Substring(0, 4)}...{cardNumber.Substring(cardNumber.Length - 4)}";
    }
}
```

#### Example: FraudDetectionDecorator

```csharp
namespace PaymentProcessorDemo.Decorators;

using Models;
using Core;

// Decorator: Fraud Detection
public class FraudDetectionDecorator : PaymentProcessorDecorator
{
    private readonly HashSet<string> _flaggedCards = new();
    private readonly Dictionary<string, int> _failedAttempts = new();

    public FraudDetectionDecorator(IPaymentProcessor innerProcessor)
        : base(innerProcessor)
    {
    }

    public override PaymentResponse Process(PaymentRequest request)
    {
        Console.WriteLine($"[FraudDetectionDecorator] Analyzing for fraud...");

        // Check if card is flagged
        if (_flaggedCards.Contains(request.CardNumber))
        {
            var errorResponse = new PaymentResponse
            {
                Success = false,
                Message = "Card flagged as potentially fraudulent",
                Amount = request.Amount,
                ProcessedAt = DateTime.UtcNow
            };

            Console.WriteLine($"[FraudDetectionDecorator] ✗ Card flagged");
            return errorResponse;
        }

        // Check for unusual amounts (> $1000)
        if (request.Amount > 1000)
        {
            Console.WriteLine($"[FraudDetectionDecorator] ⚠ Large amount detected: ${request.Amount}");
        }

        // Proceed
        var response = base.Process(request);

        // Track failed attempts
        if (!response.Success)
        {
            if (!_failedAttempts.ContainsKey(request.CardNumber))
                _failedAttempts[request.CardNumber] = 0;

            _failedAttempts[request.CardNumber]++;

            // Flag if too many failed attempts
            if (_failedAttempts[request.CardNumber] >= 3)
            {
                _flaggedCards.Add(request.CardNumber);
                Console.WriteLine($"[FraudDetectionDecorator] 🚨 Card flagged after 3 failed attempts");
            }
        }
        else
        {
            _failedAttempts[request.CardNumber] = 0; // Reset on success
        }

        Console.WriteLine($"[FraudDetectionDecorator] ✓ Fraud check passed");
        return response;
    }
}
```

### Step 3: Add to Program.cs

```csharp
// Original stack
IPaymentProcessor processor = new StripeLikePaymentProcessor();
processor = new MetricsDecorator(processor);
processor = new LoggingDecorator(processor);
processor = new RetryDecorator(processor, maxRetries: 3, delayMs: 300);
processor = new IdempotencyDecorator(processor);

// ADD NEW DECORATORS
processor = new RateLimitDecorator(processor, maxRequestsPerMinute: 5);
processor = new EncryptionDecorator(processor);
processor = new FraudDetectionDecorator(processor);

processor = new ValidationDecorator(processor);

// Now your new decorators are part of the chain!
```

## 🔄 Decorator Order Matters!

### Good Order ✓

```
ValidationDecorator (Check input first)
    ↓
IdempotencyDecorator (Cache before expensive operations)
    ↓
FraudDetectionDecorator (Detect fraud before retrying)
    ↓
RetryDecorator (Retry on errors)
    ↓
RateLimitDecorator (Rate limit before expensive API calls)
    ↓
EncryptionDecorator (Encrypt before processing)
    ↓
LoggingDecorator (Log everything)
    ↓
MetricsDecorator (Measure everything)
    ↓
StripeLikePaymentProcessor
```

### Why Order Matters

1. **Fast-fail decorators first**: Validation rejects bad input early
2. **Cache decorators early**: No need to retry cached results
3. **Security decorators**: Before expensive operations
4. **Performance decorators**: Before logging
5. **Logging/Metrics last**: Measure everything

## 🧪 Testing Decorators

### Unit Testing Example

```csharp
[TestClass]
public class RateLimitDecoratorTests
{
    [TestMethod]
    public void ShouldAllowRequestsWithinLimit()
    {
        // Arrange
        var mockProcessor = new Mock<IPaymentProcessor>();
        var decorator = new RateLimitDecorator(
            mockProcessor.Object,
            maxRequestsPerMinute: 2);

        var request1 = CreateValidRequest("4532015112830366");
        var request2 = CreateValidRequest("4532015112830366");
        var response = new PaymentResponse
        {
            Success = true,
            Message = "OK"
        };

        mockProcessor.Setup(x => x.Process(It.IsAny<PaymentRequest>()))
            .Returns(response);

        // Act
        var result1 = decorator.Process(request1);
        var result2 = decorator.Process(request2);

        // Assert
        Assert.IsTrue(result1.Success);
        Assert.IsTrue(result2.Success);
        mockProcessor.Verify(x => x.Process(It.IsAny<PaymentRequest>()),
            Times.Exactly(2));
    }

    [TestMethod]
    public void ShouldRejectRequestsExceedingLimit()
    {
        // Arrange
        var mockProcessor = new Mock<IPaymentProcessor>();
        var decorator = new RateLimitDecorator(
            mockProcessor.Object,
            maxRequestsPerMinute: 1);

        var request1 = CreateValidRequest("4532015112830366");
        var request2 = CreateValidRequest("4532015112830366");

        var response = new PaymentResponse
        {
            Success = true,
            Message = "OK"
        };

        mockProcessor.Setup(x => x.Process(It.IsAny<PaymentRequest>()))
            .Returns(response);

        // Act
        var result1 = decorator.Process(request1);
        var result2 = decorator.Process(request2);

        // Assert
        Assert.IsTrue(result1.Success);
        Assert.IsFalse(result2.Success);
        Assert.IsTrue(result2.Message.Contains("Rate limit exceeded"));

        // Process should only be called once (first request)
        mockProcessor.Verify(x => x.Process(It.IsAny<PaymentRequest>()),
            Times.Once);
    }

    private static PaymentRequest CreateValidRequest(string cardNumber)
    {
        return new PaymentRequest
        {
            IdempotencyKey = Guid.NewGuid().ToString(),
            Amount = 50m,
            CardNumber = cardNumber,
            CardholderName = "Test User",
            ExpirationDate = "12/25",
            CVC = "123"
        };
    }
}
```

## 📋 Decorator Checklist

When implementing a new decorator, ensure:

- [ ] Class extends `PaymentProcessorDecorator`
- [ ] Constructor accepts `IPaymentProcessor innerProcessor`
- [ ] Pass `innerProcessor` to base constructor
- [ ] Override `Process(PaymentRequest request)` method
- [ ] Add your behavior/logic
- [ ] Call `base.Process(request)` to continue chain
- [ ] Return `PaymentResponse`
- [ ] Provide meaningful console output/logging
- [ ] Handle exceptions appropriately
- [ ] Add unit tests
- [ ] Document the decorator's purpose

## 🎓 Common Patterns

### 1. Before-After Pattern

```csharp
public override PaymentResponse Process(PaymentRequest request)
{
    // BEFORE
    Console.WriteLine("Before processing");

    // PROCESS
    var response = base.Process(request);

    // AFTER
    Console.WriteLine("After processing");

    return response;
}
```

### 2. Guard Clause Pattern

```csharp
public override PaymentResponse Process(PaymentRequest request)
{
    if (ShouldReject(request))
    {
        return new PaymentResponse
        {
            Success = false,
            Message = "Rejected"
        };
    }

    return base.Process(request);
}
```

### 3. Try-Catch Pattern

```csharp
public override PaymentResponse Process(PaymentRequest request)
{
    try
    {
        return base.Process(request);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
        return new PaymentResponse
        {
            Success = false,
            Message = ex.Message
        };
    }
}
```

### 4. Loop/Retry Pattern

```csharp
public override PaymentResponse Process(PaymentRequest request)
{
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            return base.Process(request);
        }
        catch
        {
            if (i == maxRetries - 1) throw;
        }
    }
}
```

## 🚀 Performance Considerations

- **Decorator Stack Depth**: Deeper stacks = more overhead
- **Each decorator adds ~1ms**: Plan for cumulative time
- **Cache hits**: Dramatically reduce processing time
- **Early rejection**: ValidationDecorator saves time by failing fast

## 🔐 Security Best Practices

When adding security-related decorators:

- ✓ Never log sensitive data (card numbers, CVCs)
- ✓ Mask data in logs: `****-****-****-0366`
- ✓ Use proper encryption libraries (not custom implementations)
- ✓ Validate all inputs even after previous decorators
- ✓ Handle exceptions securely (don't leak stack traces)

---

**Now you're ready to extend the payment processor with your own decorators!** 🚀
