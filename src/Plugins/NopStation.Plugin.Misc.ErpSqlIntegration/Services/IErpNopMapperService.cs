using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.Misc.ErpSqlIntegration.Models;

namespace NopStation.Plugin.Misc.ErpSqlIntegration.Services;
public interface IErpNopMapperService
{
    Task<IList<ErpAccountDataModel>> ErpAccountMapNop(IList<ErpAccountSqlResponseModel> erpAccountSqlResponses);
    Task<IList<ErpPlaceOrderDataModel>> ErpOrderMapNop(IList<ErpOrderSyncSqlResponseModel> erpOrderSqlResponses);
    Task<IList<ErpInvoiceDataModel>> ErpInvoiceMapNop(IList<ErpInvoiceSqlResponseModel> erpInvoicesResponseData);
    Task<IList<ErpPriceSpecialPricingDataModel>> ErpPriceSpecialPricingMapNop(IList<ErpPriceSpecialPricingSqlResponseModel> erpPriceSpecialPricingsResponseData);
    Task<IList<ErpPriceGroupPricingDataModel>> ErpPriceGroupPricingMapNopAsync(IList<ErpPriceGroupPricingSqlResponseModel> erpPriceGroupPricingsResponseData);
    Task<IList<ErpProductDataModel>> ErpProductMapNop(IList<ErpProductSqlResponseModel> erpProductResponseData);
    Task<IList<ErpShipToAddressDataModel>> ErpShipToAddressMapNop(IList<ErpShipToAddressSqlResponseModel> erpShipToAddressResponseData);
    Task<IList<ErpStockDataModel>> ErpStockMapNop(IList<ErpStockSqlResponseModel> erpStockResponseData);
}
