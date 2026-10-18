using EnterpriseOrderPlatform.Modules.Identity.Contracts;

namespace EnterpriseOrderPlatform.Modules.Identity.Application;

public interface ITokenService
{
    LoginResponse CreateAccessToken(AuthenticatedUser user);
}
