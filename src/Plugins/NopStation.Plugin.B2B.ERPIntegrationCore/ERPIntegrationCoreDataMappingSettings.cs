using Nop.Core.Configuration;

namespace NopStation.Plugin.B2B.ERPIntegrationCore;
public class ERPIntegrationCoreDataMappingSettings : ISettings
{
    public string? ProductPropertiesToExclude { get; set; }
    public string? AccountPropertiesToExclude { get; set; }
    public string? OrderPropertiesToExclude { get; set; }
    public string? InvoicePropertiesToExclude { get; set; }
    public string? GroupPricePropertiesToExclude { get; set; }
    public string? SpecialPricePropertiesToExclude { get; set; }
    public string? StockPropertiesToExclude { get; set; }
    public string? ShipToAddressPropertiesToExclude { get; set; }
    public string? OrderItemPropertiesToExclude { get; set; }
}  