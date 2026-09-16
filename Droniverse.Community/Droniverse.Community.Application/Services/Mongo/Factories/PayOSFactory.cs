using Droniverse.Community.Application.Services.Mongo.Abstractions;
using Droniverse.Community.Application.Services.Mongo.PaymentProviders;
using Microsoft.Extensions.Logging;

namespace Droniverse.Community.Application.Services.Mongo.Factories;

/// <summary>
/// Concrete factory for creating PayOSPayment instances.
/// Implements the Factory Method pattern.
/// </summary>
internal class PayOSFactory : PaymentFactory
{
    private readonly PayOSPayment _payOSProvider;

    public PayOSFactory(ILogger<PaymentFactory> logger, PayOSPayment payOSProvider)
        : base(logger)
    {
        _payOSProvider = payOSProvider;
    }

    /// <summary>
    /// Creates and returns a PayOSPayment instance.
    /// </summary>
    public override IPayment CreatePayment()
    {
        Logger.LogInformation("Creating PAYOS payment");
        return _payOSProvider;
    }
}
