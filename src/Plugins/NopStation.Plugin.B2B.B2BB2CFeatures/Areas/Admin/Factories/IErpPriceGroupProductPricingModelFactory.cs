using System.Threading.Tasks;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;

public interface IErpPriceGroupProductPricingModelFactory
{
    Task<ErpPriceGroupProductPricingSearchModel> PrepareErpProductGroupPriceSearchModel(ErpPriceGroupProductPricingSearchModel searchModel, int productId);

    Task<ErpPriceGroupProductPricingListModel> PrepareErpProductGroupPriceListModel(ErpPriceGroupProductPricingSearchModel searchModel);

    Task<ErpPriceGroupProductPricingModel> PrepareErpProductGroupPriceModel(ErpPriceGroupProductPricingModel model, ErpGroupPrice erpProductPricing);
}