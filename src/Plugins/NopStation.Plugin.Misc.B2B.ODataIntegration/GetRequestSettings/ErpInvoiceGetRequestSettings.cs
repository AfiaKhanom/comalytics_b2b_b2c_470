using Nop.Core.Configuration;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.GetRequestSettings;

public class ErpInvoiceGetRequestSettings : ISettings
{
     public string? AccountNumber { get; set; }
                  
     public string? Start { get; set; }
                  
     public string? Limit { get; set; }
                  
     public string? LastChangedDate { get; set; }
                  
     public string? Location { get; set; }

     public int InvoiceSyncLimit { get; set; } = 100;
}