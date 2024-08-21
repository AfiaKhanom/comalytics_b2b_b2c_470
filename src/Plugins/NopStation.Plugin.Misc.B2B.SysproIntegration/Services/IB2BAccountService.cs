using NopStation.Plugin.B2B.ERPIntegrationCore.Model;

namespace NopStation.Plugin.Misc.B2B.SysproIntegration.Services;

public interface IB2BAccountService
{
    Task<ErpResponseModel> CreateAccountNoErpAsync(ErpCreateAccountModel erpCreateAccountModel);
    Task<ErpResponseData<IList<ErpAccountDataModel>>> GetAccountsFromErpAsync(ErpGetRequestModel erpRequest);
    Task<ErpResponseData<IList<ErpInvoiceDataModel>>> GetInvoiceByAccountNoFromErpAsync(ErpGetRequestModel erpRequest);
    Task<ErpResponseData<string>> GetInvoicePdfByteCodeByDocumentNoFromErpAsync(ErpGetRequestModel erpRequest);
    Task<ErpResponseData<IList<ErpShipToAddressDataModel>>> GetShipToAddressByAccountNumberFromErpAsync(ErpGetRequestModel erpRequest);
}