using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.Misc.B2B.SysproIntegration.Models;

namespace NopStation.Plugin.Misc.B2B.SysproIntegration.Services;

/// <summary>
/// Represents SysproIntegration manager
/// </summary>
public partial class SysproIntegrationService : ISysproIntegrationService
{
    private readonly SysproIntegrationSettings _sysproIntegrationSettings;

    public SysproIntegrationService(SysproIntegrationSettings sysproIntegrationSettings)
    {
        _sysproIntegrationSettings = sysproIntegrationSettings;
    }

    public Task<bool> IsValidSysproIntegrationSettings()
    {
        if (string.IsNullOrEmpty(_sysproIntegrationSettings.Token) ||
            string.IsNullOrEmpty(_sysproIntegrationSettings.BaseUrl))
        {
            return Task.FromResult(false);
        }

        if (!Uri.IsWellFormedUriString(_sysproIntegrationSettings.BaseUrl, UriKind.Absolute))
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }

    public async Task<SysproRequestModel> PrepareErpAccountsRequestBody(ErpGetRequestModel erpRequest)
    {
        if (erpRequest != null)
        {
            var start = int.Parse(erpRequest.Start);
            return new SysproRequestModel()
            {
                action = "SQL",
                name = "Online_GetCustomer",
                args = new ErpRequestArgs
                {
                    LastChangeDate = erpRequest.DateFrom,
                    Customer = string.IsNullOrWhiteSpace(erpRequest.AccountNumber) ? null : erpRequest.AccountNumber,
                    PageNumber = start > 0 ? start : 1,
                    RowsPerPage = erpRequest.Limit > 0 ? erpRequest.Limit : 100
                }
            };
        }

        return new SysproRequestModel();
    }
}