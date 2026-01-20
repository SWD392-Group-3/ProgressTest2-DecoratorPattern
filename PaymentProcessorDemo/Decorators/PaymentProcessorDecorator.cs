namespace PaymentProcessorDemo.Decorators;

using Core;
using Models;

// Base Decorator Class
public abstract class PaymentProcessorDecorator : IPaymentProcessor
{
    public IPaymentProcessor _innerProcessor { get; protected set; }

    protected PaymentProcessorDecorator(IPaymentProcessor innerProcessor)
    {
        _innerProcessor = innerProcessor;
    }

    public virtual PaymentResponse Process(PaymentRequest request)
    {
        return _innerProcessor.Process(request);
    }
}
