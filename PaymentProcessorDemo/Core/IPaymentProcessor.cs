namespace PaymentProcessorDemo.Core;

using Models;

// Component Interface
public interface IPaymentProcessor
{
    PaymentResponse Process(PaymentRequest request);
}
