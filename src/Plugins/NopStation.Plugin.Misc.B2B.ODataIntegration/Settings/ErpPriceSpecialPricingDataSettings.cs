using Nop.Core.Configuration;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.Settings;
public class ErpPriceSpecialPricingDataSettings : ISettings
{
    public string? AccountNumber { get; set; }
    public string? Branch { get; set; }
    public string? Sku { get; set; }
    public string? SpecialPrice { get; set; }
    public string? SellingPrice { get; set; }
    public string? PromoPrice { get; set; }
    public string? ListPrice { get; set; }
    public string? RetailPrice { get; set; }
    public string? DiscountPercentage { get; set; }
    public string? PricingNotes { get; set; }
}

