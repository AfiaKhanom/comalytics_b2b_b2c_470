using Nop.Core.Configuration;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.GetRequestSettings;

public class ErpAccountGetRequestSettings : AdditionalFiltersSettings
{
     public string? AccountNumber { get; set; }
                  
     public string? Start { get; set; }
                  
     public string? Limit { get; set; }
                  
     public string? LastChangedDate { get; set; }
                  
     public string? Location { get; set; }

     public int AccountSyncLimit { get; set; } = 100;
}