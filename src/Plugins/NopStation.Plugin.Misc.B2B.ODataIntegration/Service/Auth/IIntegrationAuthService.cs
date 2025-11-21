using Nop.Core.Domain.Customers;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.Service.Auth;

public interface IIntegrationAuthService
{
    Task<string> GetToken(Customer customer);
}
