using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.Areas.Admin.Model;
public record ErpPriceGroupPricingDataSettingsModel : ODataIntegrationModel
{
    public ErpPriceGroupPricingDataSettingsModel()
    {
        ErpGetRequestSettingsModel = new ErpGetRequestSettingsModel();
    }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpPriceGroupPricingDataSettingsModel.Fields.GroupPriceCode")]
    public string? GroupPriceCode { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpPriceGroupPricingDataSettingsModel.Fields.Sku")]
    public string? Sku { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpPriceGroupPricingDataSettingsModel.Fields.Price")]
    public string? Price { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpPriceGroupPricingDataSettingsModel.Fields.GroupPrices")]
    public string? GroupPrices { get; set; }
    public ErpGetRequestSettingsModel ErpGetRequestSettingsModel { get; set; }
}
