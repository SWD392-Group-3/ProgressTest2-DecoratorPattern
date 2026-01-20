# Payment Processor - Decorator Design Pattern Demo

## 📋 Mô Tả

Đây là một ứng dụng console .NET 8.0 minh họa **Decorator Design Pattern** với một ví dụ thực tế: **Xử lý thanh toán** với các mối quan tâm cắt ngang (cross-cutting concerns).

## 🏗️ Kiến Trúc

### Component Interface

```
IPaymentProcessor
├── Process(PaymentRequest request): PaymentResponse
```

### Concrete Component

- **StripeLikePaymentProcessor**: Giả lập cổng thanh toán Stripe
  - Xử lý thanh toán thực tế
  - Giả lập lỗi tạm thời (5% xác suất)

### Decorators (Bọc chức năng)

1. **ValidationDecorator**
   - Kiểm tra idempotency key
   - Kiểm tra amount > 0
   - Kiểm tra thông tin thẻ (số thẻ 16 chữ số, CVC 3 chữ số)
   - **Vị trí**: Bọc ngoài cùng (kiểm tra đầu tiên)

2. **IdempotencyDecorator**
   - Lưu kết quả thành công vào cache
   - Nếu request cùng key gọi lại → trả kết quả cached
   - Chống duplicate payment (gọi 2 lần cùng key không charge 2 lần)

3. **RetryDecorator**
   - Retry khi xảy ra TimeoutException
   - Configurable: số lần retry & delay
   - Mặc định: 3 lần, delay 500ms

4. **LoggingDecorator**
   - Log chi tiết trước/sau xử lý
   - Hiển thị thông tin request (che số thẻ)
   - Log lỗi nếu có
   - Đo lường thời gian xử lý

5. **MetricsDecorator**
   - Đếm tổng số lần gọi
   - Đếm số lần thành công/thất bại
   - Tính toán tỷ lệ thành công
   - Tính trung bình thời gian xử lý

### Stack Decorator (Thứ tự bọc từ trong ra)

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

## 📦 Cấu Trúc Project

```
PaymentProcessorDemo/
├── Models/
│   ├── PaymentRequest.cs       # DTO yêu cầu thanh toán
│   └── PaymentResponse.cs      # DTO phản hồi thanh toán
├── Core/
│   ├── IPaymentProcessor.cs    # Interface Component
│   └── StripeLikePaymentProcessor.cs  # Concrete Component
├── Decorators/
│   ├── PaymentProcessorDecorator.cs     # Base Decorator
│   ├── ValidationDecorator.cs
│   ├── IdempotencyDecorator.cs
│   ├── RetryDecorator.cs
│   ├── LoggingDecorator.cs
│   └── MetricsDecorator.cs
├── Program.cs                  # Điểm vào & Demo
└── PaymentProcessorDemo.csproj
```

## 🎯 Các Test Cases

### Test 1: Valid Payment ✓

- Request thanh toán hợp lệ
- Thương được xử lý thành công

### Test 2: Duplicate Request (Idempotency) ✓

- Gọi lại cùng idempotency key
- **Kết quả**: Trả kết quả cached, không xử lý lại
- **Tác dụng**: Chống double-charge khi user click button 2 lần

### Test 3: Invalid Amount ✗

- Amount <= 0
- **Kết quả**: ValidationDecorator từ chối ngay

### Test 4: Invalid Card Number ✗

- Số thẻ không đúng format
- **Kết quả**: ValidationDecorator từ chối

### Test 5: Valid Payment + Temporary Error

- Request hợp lệ, nhưng gặp lỗi tạm thời từ gateway
- **RetryDecorator** thử lại → thành công ✓
- Hiển thị retry logic hoạt động

## 🚀 Chạy Project

### Build

```bash
cd PaymentProcessorDemo
dotnet build
```

### Run

```bash
dotnet run
```

### Output ví dụ:

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

────────────────────────────────────────────────────────────
TEST 2: Duplicate Request (Same Idempotency Key)
────────────────────────────────────────────────────────────
[ValidationDecorator] Validating payment request...
[ValidationDecorator] ✓ All validations passed
[IdempotencyDecorator] Checking idempotency key: txn_001_valid
[IdempotencyDecorator] ✓ Found cached result: txn_b899b0a2

→ Result: ✓ SUCCESS - Payment processed successfully

...

============================================================
📊 PAYMENT PROCESSOR METRICS
============================================================
Total Calls:           3
Successful:            2 (66%)
Failed:                1 (33%)
Average Duration:      0ms
Total Duration:        2ms
============================================================
```

## 💡 Lợi ích của Decorator Pattern

### 1. **Single Responsibility** ✓

- Mỗi decorator chỉ có 1 trách nhiệm
- `ValidationDecorator` chỉ validate
- `LoggingDecorator` chỉ log
- `RetryDecorator` chỉ retry

### 2. **Open/Closed Principle** ✓

- Có thể thêm decorator mới mà không sửa code cũ
- Ví dụ: thêm `CacheDecorator` dễ dàng
- Mở rộng → Không sửa code hiện tại

### 3. **Flexibility** ✓

- Có thể bọc decorator theo nhiều cách
- Tắt/bật feature bằng cách bỏ decorator
- Thay đổi thứ tự decorator để thay đổi hành vi

### 4. **Readable & Maintainable** ✓

- Code dễ đọc & hiểu
- Mỗi decorator rõ ràng về chức năng
- Test riêng từng decorator dễ dàng

### 5. **Cross-cutting Concerns** ✓

- Giải quyết vấn đề validation, logging, retry, metrics
- Tất cả đều là thêm vào mà không xâm lấn core logic

## 🔄 So sánh với các Pattern khác

### vs. Inheritance (Kế thừa)

```csharp
// ❌ Inheritance → Explosion của subclasses
class ValidationPaymentProcessor : IPaymentProcessor { }
class ValidationRetryPaymentProcessor : IPaymentProcessor { }
class ValidationRetryLoggingPaymentProcessor : IPaymentProcessor { }
// → 2^n combinations!
```

### vs. Composition với Decorator ✓

```csharp
// ✓ Decorator → Flexible & Clean
var processor = new StripeLikePaymentProcessor();
processor = new ValidationDecorator(processor);
processor = new RetryDecorator(processor);
processor = new LoggingDecorator(processor);
// → Tổ hợp linh hoạt!
```

## 📌 Key Points

1. **Decorator wraps Component** → `_innerProcessor`
2. **Each decorator adds behavior** → trước/sau `base.Process()`
3. **Transparent substitution** → Client không biết có decorator
4. **Composable** → Stack nhiều decorator
5. **Same interface** → Mỗi decorator là `IPaymentProcessor`

## 🎓 Học được gì?

- ✓ Decorator Pattern structure & implementation
- ✓ Cách xử lý cross-cutting concerns
- ✓ Encapsulation & composition
- ✓ Open/Closed Principle
- ✓ Real-world payment processing example
- ✓ Logging, metrics, retry, idempotency, validation

---

**Framework**: .NET 8.0  
**Ngôn ngữ**: C# 12.0  
**Pattern**: Decorator (Structural)
