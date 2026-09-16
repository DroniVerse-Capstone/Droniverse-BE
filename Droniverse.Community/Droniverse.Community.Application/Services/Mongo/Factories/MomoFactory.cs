using Droniverse.Community.Application.Services.Mongo.Abstractions;
using Droniverse.Community.Application.Services.Mongo.PaymentProviders;
using Microsoft.Extensions.Logging;

namespace Droniverse.Community.Application.Services.Mongo.Factories;

/// <summary>
/// Concrete factory for creating MomoPayment instances.
/// Implements the Factory Method pattern.
/// </summary>
internal class MomoFactory : PaymentFactory
{
    private readonly MomoPayment _momoProvider;

    public MomoFactory(ILogger<PaymentFactory> logger, MomoPayment momoProvider)
        : base(logger)
    {
        _momoProvider = momoProvider;
    }

    /// <summary>
    /// Creates and returns a MomoPayment instance.
    /// </summary>
    public override IPayment CreatePayment()
    {
        Logger.LogInformation("Creating MOMO payment");
        return _momoProvider;
    }
}
