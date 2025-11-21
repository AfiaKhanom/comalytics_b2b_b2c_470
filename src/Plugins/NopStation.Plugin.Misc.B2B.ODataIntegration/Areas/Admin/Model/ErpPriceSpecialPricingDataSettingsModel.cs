using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.Areas.Admin.Model;
public record ErpPriceSpecialPricingDataSettingsModel : ODataIntegrationModel
{
    public ErpPriceSpecialPricingDataSettingsModel()
    {
        ErpGetRequestSettingsModel = new ErpGetRequestSettingsModel();
    }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpPriceSpecialPricingDataSettingsModel.Fields.AccountNumber")]
    public string? AccountNumber { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpPriceSpecialPricingDataSettingsModel.Fields.Branch")]
    public string? Branch { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpPriceSpecialPricingDataSettingsModel.Fields.Sku")]
    public string? Sku { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpPriceSpecialPricingDataSettingsModel.Fields.SpecialPrice")]
    public string? SpecialPrice { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpPriceSpecialPricingDataSettingsModel.Fields.SellingPrice")]
    public string? SellingPrice { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpPriceSpecialPricingDataSettingsModel.Fields.PromoPrice")]
    public string? PromoPrice { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpPriceSpecialPricingDataSettingsModel.Fields.ListPrice")]
    public string? ListPrice { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpPriceSpecialPricingDataSettingsModel.Fields.RetailPrice")]
    public string? RetailPrice { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpPriceSpecialPricingDataSettingsModel.Fields.DiscountPercentage")]
    public string? DiscountPercentage { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpPriceSpecialPricingDataSettingsModel.Fields.PricingNotes")]
    public string? PricingNotes { get; set; }

    public ErpGetRequestSettingsModel ErpGetRequestSettingsModel { get; set; }
}

