using EnterpriseOrderPlatform.BuildingBlocks;
using EnterpriseOrderPlatform.Modules.Ordering.Application;

namespace EnterpriseOrderPlatform.Modules.Ordering.Infrastructure;

/// <summary>Development stand-in for a real payment provider. Declines amounts above 5000.</summary>
internal sealed class FakePaymentProvider : IPaymentProvider
{
    public Task<Result<string>> ChargeAsync(
        Guid orderId, decimal amount, string currency, string idempotencyKey, CancellationToken cancellationToken)
    {
        if (amount > 5000m)
        {
            return Task.FromResult(Result.Failure<string>(new ResultError("payment.declined", "The payment was declined.")));
        }

        return Task.FromResult(Result.Success("fake-" + Guid.NewGuid().ToString("N")));
    }
}
