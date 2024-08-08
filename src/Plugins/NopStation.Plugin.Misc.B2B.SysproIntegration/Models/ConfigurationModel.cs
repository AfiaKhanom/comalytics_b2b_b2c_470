using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.B2B.SysproIntegration.Models;
public class ConfigurationModel
{
    [NopResourceDisplayName("NopStation.Plugin.Misc.B2B.SysproIntegration.Fields.BaseUrl")]
    public string BaseUrl { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.Misc.B2B.SysproIntegration.Fields.Token")]
    public string Token { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.Misc.B2B.SysproIntegration.Fields.HttpCallMaxRetries")]
    public int HttpCallMaxRetries { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.Misc.B2B.SysproIntegration.Fields.HttpCallRestTimeInSeconds")]
    public int HttpCallRestTimeInSeconds { get; set; }

    public bool HideGeneralBlock { get; set; }
}
