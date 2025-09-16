using Nop.Core.Domain.Customers;

namespace NopStation.Plugin.Misc.ErpSqlIntegration.Services.Auth;

public interface IIntegrationAuthService
{
    string GetToken(Customer customer);
}
