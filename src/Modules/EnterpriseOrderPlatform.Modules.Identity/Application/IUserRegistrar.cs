using EnterpriseOrderPlatform.BuildingBlocks;

namespace EnterpriseOrderPlatform.Modules.Identity.Application;

/// <summary>Creates a user with the Customer role. Implemented by the Infrastructure layer.</summary>
public interface IUserRegistrar
{
    Task<Result<Guid>> RegisterCustomerAsync(string email, string password, CancellationToken cancellationToken);
}
