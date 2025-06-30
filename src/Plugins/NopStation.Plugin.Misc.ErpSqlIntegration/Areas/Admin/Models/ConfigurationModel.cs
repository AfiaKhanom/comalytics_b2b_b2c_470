using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.ErpSqlIntegration.Areas.Admin.Models;
public class ConfigurationModel
{
    [NopResourceDisplayName("NopStation.Plugin.Misc.ErpSqlIntegration.Fields.HttpCallMaxRetries")]
    public int HttpCallMaxRetries { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.Misc.ErpSqlIntegration.Fields.HttpCallRestTimeInSeconds")]
    public int HttpCallRestTimeInSeconds { get; set; }    
    
    [NopResourceDisplayName("NopStation.Plugin.Misc.ErpSqlIntegration.Fields.ConnectionString")]
    public string ConnectionString { get; set; }

    public bool HideGeneralBlock { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.Misc.ErpSqlIntegration.Fields.BaseUrl")]
    public string BaseUrl { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.Misc.ErpSqlIntegration.Fields.AuthUserName")]
    public string AuthUserName { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.Misc.ErpSqlIntegration.Fields.AuthPassword")]
    public string AuthPassword { get; set; }
}
