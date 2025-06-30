using NopStation.Plugin.B2B.ERPIntegrationCore.Model;

namespace NopStation.Plugin.Misc.ErpSqlIntegration.Services;
public interface IShipToAddressService
{
    Task<ErpResponseData<IList<ErpShipToAddressDataModel>>> GetShipToAddressFromErpAsync(ErpGetRequestModel erpRequest);
}
