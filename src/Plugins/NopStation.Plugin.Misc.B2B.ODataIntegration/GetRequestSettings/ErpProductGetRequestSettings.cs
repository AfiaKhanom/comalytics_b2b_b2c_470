using Nop.Core.Configuration;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.GetRequestSettings;
public class ErpProductGetRequestSettings : AdditionalFiltersSettings
{
    public string? ProductSku { get; set; }

    public string? Start { get; set; }

    public string? Limit { get; set; }

    public string? LastChangedDate { get; set; }

    public int ProductSyncLimit { get; set; } = 100;
}