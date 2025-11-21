using Nop.Core.Configuration;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.GetRequestSettings;

public class ErpInvoicePdfGetRequestSettings : ISettings
{
     public string? AccountNumber { get; set; }
                  
     public string? DocumentNumber { get; set; }
                  
     public string? OrderNumber { get; set; }
                  
     public string? ProductSku { get; set; }
                  
     public string? Start { get; set; }
                  
     public string? Limit { get; set; }
                  
     public string? LastChangedDate { get; set; }
                  
     public string? Location { get; set; }
                  
     public string? PriceCode { get; set; }
}