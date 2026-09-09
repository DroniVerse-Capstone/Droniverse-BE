using Droniverse.Community.Application.Services.Mongo.Abstractions;
using Droniverse.Community.Application.Services.Mongo.PaymentProviders;
using Microsoft.Extensions.Logging;

namespace Droniverse.Community.Application.Services.Mongo.Factories;

/// <summary>
/// Concrete factory for creating CashPayment instances.
/// Implements the Factory Method pattern.
/// </summary>
internal class CashFactory : PaymentFactory
{
    private readonly CashPayment _cashProvider;

    public CashFactory(ILogger<PaymentFactory> logger, CashPayment cashProvider)
        : base(logger)
    {
        _cashProvider = cashProvider;
    }

    /// <summary>
    /// Creates and returns a CashPayment instance.
    /// </summary>
    public override IPayment CreatePayment()
    {
        Logger.LogInformation("Creating CASH payment");
        return _cashProvider;
    }
}
