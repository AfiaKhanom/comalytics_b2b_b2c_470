using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.Misc.B2B.SysproIntegration.Models;

namespace NopStation.Plugin.Misc.B2B.SysproIntegration.Services;
public interface IErpNopMapperService
{
    Task<IList<ErpAccountDataModel>> ErpAccountMapNop(IList<ErpAccountSysproResponseModel> erpAccountSysproResponses);
    Task<IList<ErpPriceSpecialPricingDataModel>> ErpPriceSpecialPricingMapNop(List<ErpPriceSpecialPricingSysproResponseModel> erpPriceSpecialPricingsResponseData);
    Task<IList<ErpProductDataModel>> ErpProductMapNop(IList<ErpProductSysproResponseModel> erpProductResponseData);
    //Task<IList<ErpInvoiceDataModel>> ErpInvoiceMapNop(IList<ProcessingDocumentModel> erpInvoicesResponse);
    //Task<IList<ErpProductDataModel>> ErpStockMapNop(IList<ErpStockRecordModel> erpStockResponses);
    //Task<IList<ErpShipToAddressDataModel>> ErpShipToAddressMapNop(IList<DebtorsMasterModel> erpShipToAddressResponse);
    //Task<IList<ErpPriceGroupPricingDataModel>> ErpGroupPriceMapNop(IList<ErpStockRecordModel> erpGroupPriceResponses);
    //Task<IList<ErpPriceSpecialPricingDataModel>> ErpSpecialPriceMapNop(IList<ErpStockRecordModel> erpSpecialPriceResponses);
    //Task<IList<ErpPlaceOrderDataModel>> ErpOrderMapNop(IList<ProcessingDocumentModel> erpOrdersResponse);
}
