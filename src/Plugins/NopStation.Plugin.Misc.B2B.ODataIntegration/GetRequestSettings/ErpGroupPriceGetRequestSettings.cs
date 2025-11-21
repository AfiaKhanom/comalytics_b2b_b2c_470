using Nop.Core.Configuration;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.GetRequestSettings;
public class ErpGroupPriceGetRequestSettings : ISettings
{
    public string? Start { get; set; }

    public string? Limit { get; set; }

    public string? LastChangedDate { get; set; }
    public string? DateFrom { get; set; }
    public string? DateTo { get; set; }

    public string? Location { get; set; }

    public string? PriceCode { get; set; }
    public string? ProductSku { get; set; }

    public int GroupPriceSyncLimit { get; set; } = 100;
}
