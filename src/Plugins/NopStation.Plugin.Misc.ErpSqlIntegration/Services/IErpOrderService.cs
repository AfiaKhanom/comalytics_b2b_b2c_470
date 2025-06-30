using NopStation.Plugin.B2B.ERPIntegrationCore.Model;

namespace NopStation.Plugin.Misc.ErpSqlIntegration.Services;
public interface IErpOrderService
{
    Task<ErpResponseModel> CreateOrderOnErpAsync(ErpPlaceOrderDataModel erpPlaceOrderDataModel);
    Task<ErpResponseData<IList<ErpPlaceOrderDataModel>>> GetOrderByAccountFromErpAsync(ErpGetRequestModel erpRequest);
}
