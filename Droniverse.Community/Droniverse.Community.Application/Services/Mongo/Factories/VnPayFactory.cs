using Droniverse.Community.Application.Services.Mongo.Abstractions;
using Droniverse.Community.Application.Services.Mongo.PaymentProviders;
using Microsoft.Extensions.Logging;

namespace Droniverse.Community.Application.Services.Mongo.Factories;

/// <summary>
/// Concrete factory for creating VnPayPayment instances.
/// Implements the Factory Method pattern.
/// </summary>
internal class VnPayFactory : PaymentFactory
{
    private readonly VnPayPayment _vnPayProvider;

    public VnPayFactory(ILogger<PaymentFactory> logger, VnPayPayment vnPayProvider)
        : base(logger)
    {
        _vnPayProvider = vnPayProvider;
    }

    /// <summary>
    /// Creates and returns a VnPayPayment instance.
    /// </summary>
    public override IPayment CreatePayment()
    {
        Logger.LogInformation("Creating VNPAY payment");
        return _vnPayProvider;
    }
}
