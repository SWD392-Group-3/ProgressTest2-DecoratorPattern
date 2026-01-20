# Payment Processor - Decorator Design Pattern Demo

## 📋 Description

This is a .NET 8.0 console application demonstrating the **Decorator Design Pattern** with a real-world example: **Payment Processing** with multiple cross-cutting concerns.

## 🏗️ Architecture

### Component Interface

```
IPaymentProcessor
├── Process(PaymentRequest request): PaymentResponse
```

### Concrete Component

- **StripeLikePaymentProcessor**: Simulates Stripe payment gateway
  - Handles actual payment processing
  - Simulates temporary errors (5% probability)

### Decorators (Adding Functionality)

1. **ValidationDecorator**
   - Validates idempotency key
   - Checks amount > 0
   - Validates card info (16-digit card number, 3-digit CVC)
   - **Position**: Outermost (first validation)

2. **IdempotencyDecorator**
   - Caches successful results
   - Returns cached result for duplicate requests (same key)
   - Prevents double-charging on duplicate clicks

3. **RetryDecorator**
   - Retries on TimeoutException
   - Configurable: retry count & delay
   - Default: 3 retries, 500ms delay

4. **LoggingDecorator**
   - Detailed logging before/after processing
   - Displays request info (masked card number)
   - Logs errors if any
   - Measures processing duration

5. **MetricsDecorator**
   - Counts total calls
   - Counts successful/failed calls
   - Calculates success rate
   - Calculates average processing time

### Decorator Stack (Order from innermost to outermost)

```
ValidationDecorator
    ↓
IdempotencyDecorator
    ↓
RetryDecorator
    ↓
LoggingDecorator
    ↓
MetricsDecorator
    ↓
StripeLikePaymentProcessor (Concrete Component)
```

## 📦 Project Structure

```
PaymentProcessorDemo/
├── Models/
│   ├── PaymentRequest.cs       # Payment request DTO
│   └── PaymentResponse.cs      # Payment response DTO
├── Core/
│   ├── IPaymentProcessor.cs    # Component interface
│   └── StripeLikePaymentProcessor.cs  # Concrete component
├── Decorators/
│   ├── PaymentProcessorDecorator.cs     # Base decorator
│   ├── ValidationDecorator.cs
│   ├── IdempotencyDecorator.cs
│   ├── RetryDecorator.cs
│   ├── LoggingDecorator.cs
│   └── MetricsDecorator.cs
├── Program.cs                  # Entry point & Demo
└── PaymentProcessorDemo.csproj
```

## 🎯 Test Cases

### Test 1: Valid Payment ✓

- Valid payment request
- Usually processes successfully

### Test 2: Duplicate Request (Idempotency) ✓

- Calls with the same idempotency key again
- **Result**: Returns cached result, no reprocessing
- **Purpose**: Prevents double-charge on double-click

### Test 3: Invalid Amount ✗

- Amount <= 0
- **Result**: ValidationDecorator rejects immediately

### Test 4: Invalid Card Number ✗

- Card number in wrong format
- **Result**: ValidationDecorator rejects

### Test 5: Valid Payment + Temporary Error

- Valid request but gateway experiences temporary error
- **RetryDecorator** retries → succeeds ✓
- Demonstrates retry logic in action

## 🚀 Running the Project

### Build

```bash
cd PaymentProcessorDemo
dotnet build
```

### Run

```bash
dotnet run
```

### Example Output

```
╔═══════════════════════════════════════════════════════════╗
║   PAYMENT PROCESSOR - DECORATOR DESIGN PATTERN DEMO       ║
╚═══════════════════════════════════════════════════════════╝

────────────────────────────────────────────────────────────
TEST 1: Valid Payment
────────────────────────────────────────────────────────────
[ValidationDecorator] Validating payment request...
[ValidationDecorator] ✓ All validations passed
[IdempotencyDecorator] Checking idempotency key: txn_001_valid
[IdempotencyDecorator] New request, processing...
[RetryDecorator] Starting retry logic (max retries: 3)
[LoggingDecorator] ▶ START processing payment
[LoggingDecorator]   Amount: $99.99
[LoggingDecorator]   Card: ****-****-****-0366
[LoggingDecorator]   Timestamp: 2026-01-20 08:46:12.170
[StripeLikePaymentProcessor] Processing payment of $99.99...
[StripeLikePaymentProcessor] ✓ Transaction ID: txn_b899b0a2
[MetricsDecorator] Call #1 completed in 2.70ms
[LoggingDecorator] ◀ END processing payment
[LoggingDecorator]   Success: True
[LoggingDecorator]   Message: Payment processed successfully
[LoggingDecorator]   Duration: 23.97ms
[IdempotencyDecorator] ✓ Cached result for key: txn_001_valid

→ Result: ✓ SUCCESS - Payment processed successfully
```

## 💡 Benefits of Decorator Pattern

### 1. **Single Responsibility** ✓

- Each decorator has ONE responsibility
- `ValidationDecorator` only validates
- `LoggingDecorator` only logs
- `RetryDecorator` only retries

### 2. **Open/Closed Principle** ✓

- Can add new decorators without modifying existing code
- Example: Add `CacheDecorator` easily
- Extend → Don't modify existing code

### 3. **Flexibility** ✓

- Stack decorators in multiple ways
- Enable/disable features by adding/removing decorators
- Change decorator order to change behavior

### 4. **Readable & Maintainable** ✓

- Code is easy to read and understand
- Each decorator has clear responsibility
- Easy to test each decorator independently

### 5. **Cross-cutting Concerns** ✓

- Addresses validation, logging, retry, metrics
- All added without invading core logic

## 🔄 Comparison with Other Patterns

### vs. Inheritance (The Problem)

```csharp
// ❌ Inheritance → Explosion of subclasses
class ValidationPaymentProcessor : IPaymentProcessor { }
class ValidationRetryPaymentProcessor : IPaymentProcessor { }
class ValidationRetryLoggingPaymentProcessor : IPaymentProcessor { }
// → 2^n combinations!
```

### vs. Composition with Decorator ✓

```csharp
// ✓ Decorator → Flexible & Clean
var processor = new StripeLikePaymentProcessor();
processor = new ValidationDecorator(processor);
processor = new RetryDecorator(processor);
processor = new LoggingDecorator(processor);
// → Flexible combinations!
```

## 📌 Key Points

1. **Decorator wraps Component** → `_innerProcessor`
2. **Each decorator adds behavior** → before/after `base.Process()`
3. **Transparent substitution** → Client doesn't know about decorators
4. **Composable** → Stack multiple decorators
5. **Same interface** → Each decorator is an `IPaymentProcessor`

## 🎓 Learning Outcomes

- ✓ Decorator Pattern structure & implementation
- ✓ Handling cross-cutting concerns
- ✓ Encapsulation & composition
- ✓ Open/Closed Principle
- ✓ Real-world payment processing example
- ✓ Logging, metrics, retry, idempotency, validation patterns

---

**Framework**: .NET 8.0  
**Language**: C# 12.0  
**Pattern**: Decorator (Structural)
