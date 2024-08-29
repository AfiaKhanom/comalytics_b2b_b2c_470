using NopStation.Plugin.B2B.ERPIntegrationCore.Model;

namespace NopStation.Plugin.Misc.B2B.SysproIntegration.Services;
public interface ISysproIntegrationService
{
    Task<dynamic> PrepareErpAccountsRequestBody(ErpGetRequestModel erpRequest);
    Task<bool> IsValidSysproIntegrationSettings();
    Task<dynamic> PrepareErpProductRequestBody(ErpGetRequestModel erpRequest);
    Task<dynamic> PrepareErpPriceSpecialPricingsRequestBody(ErpGetRequestModel erpRequest);
    Task<dynamic> PrepareErpStockRequestBody(ErpGetRequestModel erpRequest);
    Task<dynamic> PrepareErpShipToAddressRequestBody(ErpGetRequestModel erpRequest);
}
