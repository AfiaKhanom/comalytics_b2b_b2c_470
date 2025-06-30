using NopStation.Plugin.B2B.ERPIntegrationCore.Model;

namespace NopStation.Plugin.Misc.ErpSqlIntegration.Services;
public interface IB2BStockService
{
    Task<ErpResponseData<IList<ErpStockDataModel>>> GetStockFromErpAsync(ErpGetRequestModel erpRequest);
}
