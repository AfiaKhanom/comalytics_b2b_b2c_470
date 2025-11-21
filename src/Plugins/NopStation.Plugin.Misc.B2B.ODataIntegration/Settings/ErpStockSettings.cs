using Nop.Core.Configuration;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.Settings;
public class ErpStockSettings : ISettings
{
    public string? WarehouseNameOrCode { get; set; }
    public string? Sku { get; set; }
    public string? SalesOrgCode { get; set; }
    public string? QuantityOnHand { get; set; }
    public string? QuantityOnSalesOrder { get; set; }
    public string? LastChangedDate { get; set; }
}