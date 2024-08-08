using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.Misc.B2B.SysproIntegration.Models;

namespace NopStation.Plugin.Misc.B2B.SysproIntegration.Services;
public interface ISysproIntegrationService
{
    Task<SysproRequestModel> PrepareErpAccountsRequestBody(ErpGetRequestModel erpRequest);

    Task<bool> IsValidSysproIntegrationSettings();
}
