using Nop.Core.Configuration;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.Settings;
public class ErpPriceGroupPricingDataSettings : ISettings
{
    public string? GroupPriceCode { get; set; }
    public string? Sku { get; set; }
    public string? Price { get; set; }
    public string? GroupPrices { get; set; }
}

