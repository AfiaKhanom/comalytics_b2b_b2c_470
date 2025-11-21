using Nop.Core.Configuration;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.GetRequestSettings;

public class ErpStockGetRequestSettings : ISettings
{
     public string? ProductSku { get; set; }
                  
     public string? Start { get; set; }
                  
     public string? Limit { get; set; }
                  
     public string? LastChangedDate { get; set; }
                  
     public string? WarehouseCode { get; set; }
     public int StockSyncLimit { get; set; } = 100;
}