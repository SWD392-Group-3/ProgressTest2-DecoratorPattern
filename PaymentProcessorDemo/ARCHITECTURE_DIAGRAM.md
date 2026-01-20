# Decorator Pattern - Visual Explanation

## 🏗️ Class Diagram

```
┌────────────────────────────────────┐
│     <<interface>>                  │
│    IPaymentProcessor               │
├────────────────────────────────────┤
│ + Process(request): Response       │
└────────────────────────────────────┘
           ▲
           │
           │ implements
           │
    ┌──────┴──────┐
    │             │
    │             │
┌───┴────────────────────────┐    ┌──────────────────────────────┐
│ StripeLikePaymentProcessor │    │ PaymentProcessorDecorator    │
├───────────────────────────┤    ├──────────────────────────────┤
│ + Process(request)        │    │ # _innerProcessor            │
│   - Call Stripe API       │    │ + Process(request)           │
│   - Simulate errors (5%)  │    │   return _innerProcessor...  │
└───────────────────────────┘    └──────────────────────────────┘
                                          ▲
                                          │ extends
                                          │
                    ┌─────────────────────┼─────────────────────┐
                    │                     │                     │
                    │                     │                     │
        ┌───────────┴──────────┐  ┌──────┴─────────────┐  ┌────┴──────────────┐
        │ ValidationDecorator  │  │ IdempotencyDeco... │  │ RetryDecorator     │
        ├─────────────────────┤  ├───────────────────┤  ├────────────────────┤
        │ + Process(request)  │  │ - _cache: Dict    │  │ - _maxRetries: int │
        │   - Validate input  │  │ + Process(request)│  │ - _delayMs: int    │
        │   - Check card      │  │   - Check cache   │  │ + Process(request) │
        │   - Check amount    │  │   - Retry if err  │  │   - Retry on error │
        └─────────────────────┘  └───────────────────┘  └────────────────────┘
                                          │
                    ┌─────────────────────┼─────────────────────┐
                    │                     │                     │
        ┌───────────┴──────────┐  ┌──────┴─────────────┐
        │ LoggingDecorator     │  │ MetricsDecorator   │
        ├─────────────────────┤  ├───────────────────┤
        │ + Process(request)  │  │ - _totalCalls     │
        │   - Log start       │  │ - _successCount   │
        │   - Log duration    │  │ - _failureCount   │
        │   - Log errors      │  │ + Process(request)│
        └─────────────────────┘  │ + PrintMetrics()  │
                                  └───────────────────┘
```

## 🔄 Flow Diagram - Request Processing

```
Client Creates PaymentRequest
         │
         ▼
ValidationDecorator.Process()
         │
         ├─ Validate card, amount, etc.
         │  ├─ Invalid? → Return Error Response
         │  └─ Valid? → Continue
         │
         ▼
IdempotencyDecorator.Process()
         │
         ├─ Check cache with idempotency key
         │  ├─ Found? → Return cached response
         │  └─ Not found? → Continue
         │
         ▼
RetryDecorator.Process()
         │
         ├─ Try to process (up to 3 times)
         │  ├─ Success? → Continue
         │  └─ Timeout? → Retry with delay
         │
         ▼
LoggingDecorator.Process()
         │
         ├─ Log: START processing
         │ ├─ Amount, Card (masked), Timestamp
         │ │
         │ ▼
         │ MetricsDecorator.Process()
         │ │
         │ └─ Call actual processor
         │    └─ StripeLikePaymentProcessor.Process()
         │       └─ Simulate Stripe API call
         │
         ├─ Log: END processing / ERROR
         │ └─ Success, Duration, Exception info
         │
         ▼
PaymentResponse returned to IdempotencyDecorator
         │
         ├─ Cache successful result
         │
         ▼
Response returned to Client
```

## 📊 Execution Sequence - Valid Payment Example

```
Timeline:
┌──────────────────────────────────────────────────────────┐
│                    VALID PAYMENT TEST                    │
└──────────────────────────────────────────────────────────┘

1️⃣  ValidationDecorator
    Time: 0ms
    ✓ Validate idempotency key: txn_001_valid
    ✓ Validate amount: $99.99
    ✓ Validate card: 4532015112830366
    ✓ All valid → proceed

2️⃣  IdempotencyDecorator
    Time: 0.1ms
    ✓ Check cache: txn_001_valid
    ✓ Key not found → new request

3️⃣  RetryDecorator
    Time: 0.2ms
    ✓ Try attempt 1 → proceed

4️⃣  LoggingDecorator
    Time: 0.3ms
    ▶ START processing payment
    └─ Amount: $99.99, Card: ****-****-****-0366

5️⃣  MetricsDecorator
    Time: 0.5ms
    ▶ Call #1

6️⃣  StripeLikePaymentProcessor
    Time: 1.0ms
    ✓ Processing payment of $99.99
    ✓ Transaction ID: txn_b899b0a2

7️⃣  MetricsDecorator (return)
    Time: 2.7ms
    ✓ Call #1 completed in 2.70ms

8️⃣  LoggingDecorator (return)
    Time: 24ms
    ◀ END processing payment
    └─ Success: True, Message: "Payment processed successfully"
       Duration: 23.97ms

9️⃣  IdempotencyDecorator (return)
    Time: 24.2ms
    ✓ Cached result for key: txn_001_valid

🔟 Response to Client
    Time: 24.3ms
    ✓ PaymentResponse { Success=true, TransactionId="txn_b899b0a2" }

═════════════════════════════════════════════════════════════
TOTAL PROCESSING TIME: ~24ms
═════════════════════════════════════════════════════════════
```

## 🔄 Idempotency Test - Duplicate Request

```
FIRST REQUEST (txn_001_valid):
ValidationDecorator
    ↓ Validates
IdempotencyDecorator
    ├─ Cache miss
    ├─ Proceeds to process
    ↓
[REST OF DECORATORS + ACTUAL PROCESSOR]
    ↓
IdempotencyDecorator
    ├─ Stores in cache: {txn_001_valid → Response}
    ↓
Response returned

SECOND REQUEST (same key: txn_001_valid):
ValidationDecorator
    ↓ Validates
IdempotencyDecorator
    ├─ Cache HIT! 🎯
    ├─ Return cached response immediately
    ├─ NO further processing
    ↓
Response returned (FROM CACHE)

✓ RESULT: Same transaction ID, no duplicate charge!
✓ PROCESSING TIME: <1ms (from cache)
```

## 💡 Retry Logic Explanation

```
Scenario: Temporary gateway error (5% probability)

RetryDecorator Process:
│
├─ Attempt 1
│  ├─ Call processor
│  ├─ TimeoutException thrown ❌
│  └─ caught
│
├─ Wait 300ms ⏳
│
├─ Attempt 2
│  ├─ Call processor
│  ├─ TimeoutException thrown ❌
│  └─ caught
│
├─ Wait 300ms ⏳
│
├─ Attempt 3
│  ├─ Call processor
│  ├─ Success! ✓
│  └─ Return response
│
└─ Result: Success after 3 attempts


Configuration:
- maxRetries = 3 → try up to 3 times
- delayMs = 500 → wait 500ms between attempts
- totalAttempts = 1 (success) + 2 (failures) = 3
```

## 🎯 Metrics Collection

```
After all tests complete:

MetricsDecorator Statistics:

Total Calls:           5
├─ Test 1 (valid):    1 call (success)
├─ Test 2 (duplicate): 0 calls (cached)
├─ Test 3 (invalid):  0 calls (validation rejected)
├─ Test 4 (bad card): 0 calls (validation rejected)
└─ Test 5 (retry):    2 calls (1 failure + 1 success)

Successful:            2 (66%)
Failed:                1 (33%)
├─ Failed at attempt 1 (then retried successfully)
└─ Invalid validations don't count as "failures"

Average Duration:      ~8ms
├─ Successful calls average
├─ Includes all decorator overhead

Total Duration:        ~32ms
└─ Combined processing time for all calls
```

## 🏃 Performance Flow

```
Fast Path (Valid + No Errors):
┌─────────────────────────────────────────────┐
│ 1. Validation (< 1ms)                       │
│ 2. Idempotency check (< 1ms)                │
│ 3. Retry setup (< 1ms)                      │
│ 4. Logging overhead (< 1ms)                 │
│ 5. Metrics recording (< 1ms)                │
│ 6. Actual processing (1-2ms)                │
├─────────────────────────────────────────────┤
│ TOTAL: ~5-10ms                              │
└─────────────────────────────────────────────┘

Slow Path (With Retry):
┌─────────────────────────────────────────────┐
│ 1. Validation (< 1ms)                       │
│ 2. Idempotency check (< 1ms)                │
│ 3. Retry attempt 1 - FAILS (1ms)            │
│ 4. Wait 300ms ⏳                             │
│ 5. Retry attempt 2 - SUCCESS (1-2ms)        │
│ 6. Logging overhead (< 1ms)                 │
│ 7. Metrics recording (< 1ms)                │
├─────────────────────────────────────────────┤
│ TOTAL: ~305ms (mostly waiting!)             │
└─────────────────────────────────────────────┘

Cached Path (Duplicate Request):
┌─────────────────────────────────────────────┐
│ 1. Validation (< 1ms)                       │
│ 2. Idempotency check - CACHE HIT (< 0.5ms) │
│ 3. Return cached response                   │
├─────────────────────────────────────────────┤
│ TOTAL: < 1ms ⚡                             │
└─────────────────────────────────────────────┘
```

---

## 📝 Key Takeaways

1. **Layers of Responsibility**: Each decorator handles one concern
2. **Composability**: Decorators can be stacked in any order (mostly)
3. **Transparency**: Client code doesn't need to know about decorators
4. **Extensibility**: Add new decorators without changing existing code
5. **Separation of Concerns**: Business logic separate from cross-cutting concerns
