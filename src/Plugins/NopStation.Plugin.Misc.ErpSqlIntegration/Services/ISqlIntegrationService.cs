using NopStation.Plugin.B2B.ERPIntegrationCore.Model;

namespace NopStation.Plugin.Misc.ErpSqlIntegration.Services;
public interface ISqlIntegrationService
{
    Task<dynamic> PrepareErpAccountsRequestBody(ErpGetRequestModel erpRequest);
    Task<dynamic> PrepareErpAccountCreditsRequestBody(ErpGetRequestModel erpRequest);
    Task<bool> IsValidSqlIntegrationSettings();
    Task<(bool IsValid, string ErrorMessage)> IsValidSqlIntegrationSettingsForPlacingOrder();
    Task<dynamic> PrepareErpProductRequestBody(ErpGetRequestModel erpRequest);
    Task<dynamic> PrepareErpPriceSpecialPricingsRequestBody(ErpGetRequestModel erpRequest);
    Task<dynamic> PrepareErpStockRequestBody(ErpGetRequestModel erpRequest);
    Task<dynamic> PrepareErpShipToAddressRequestBody(ErpGetRequestModel erpRequest);
    Task<dynamic> PrepareErpOrderPlaceRequestBody(ErpPlaceOrderDataModel erpRequest);
    Task<dynamic> PrepareErpInvoiceHexRequestBody(ErpGetRequestModel erpRequest);
    Task<dynamic> PrepareErpInvoiceRequestBody(ErpGetRequestModel erpRequest);
    Task<dynamic> PrepareErpStatementHexRequestBody(ErpGetRequestModel erpRequest);
    Task<dynamic> PrepareErpDealsRequestBody(ErpGetRequestModel erpRequest);
    Task<dynamic> PrepareErpOrdersRequestBody(ErpGetRequestModel erpRequest);
    Task<dynamic> PrepareErpOrderPlaceRequestBodyAsync(ErpPlaceOrderDataModel erpRequest);
    (bool IsValid, string ErrorMessage) ValidateAdditionalHardCodedValuesSettings(string jsonSetting);
}
