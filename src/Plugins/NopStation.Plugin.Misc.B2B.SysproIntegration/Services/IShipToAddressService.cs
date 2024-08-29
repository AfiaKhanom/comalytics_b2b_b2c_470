using NopStation.Plugin.B2B.ERPIntegrationCore.Model;

namespace NopStation.Plugin.Misc.B2B.SysproIntegration.Services;
public interface IShipToAddressService
{
    Task<ErpResponseData<IList<ErpShipToAddressDataModel>>> GetShipToAddressFromErpAsync(ErpGetRequestModel erpRequest);
}
