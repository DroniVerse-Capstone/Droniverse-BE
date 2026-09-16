using Droniverse.Community.Application.Services.Mongo.Abstractions;
using Microsoft.Extensions.Logging;

namespace Droniverse.Community.Application.Services.Mongo.Factories;

/// <summary>
/// Abstract factory class for creating payment methods.
/// Each payment method has its own concrete factory implementation.
/// </summary>
internal abstract class PaymentFactory
{
    protected readonly ILogger<PaymentFactory> Logger;

    protected PaymentFactory(ILogger<PaymentFactory> logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Factory method that creates and returns a payment instance.
    /// </summary>
    public abstract IPayment CreatePayment();
}
