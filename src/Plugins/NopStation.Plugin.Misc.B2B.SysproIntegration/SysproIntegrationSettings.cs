using Nop.Core.Configuration;

namespace NopStation.Plugin.Misc.B2B.SysproIntegration;

/// <summary>
/// Represents a plugin settings
/// </summary>
public class SysproIntegrationSettings : ISettings
{
    /// <summary>
    /// Gets or sets the syspro url
    /// </summary>
    public string BaseUrl { get; set; }

    /// <summary>
    /// Gets or sets the token
    /// </summary>
    public string Token { get; set; }

    /// <summary>
    /// Gets or sets the ErpCallTimeOut
    /// </summary>
    public int ErpCallTimeOut { get; set; }
    public int HttpCallMaxRetries { get; set; }
    public int HttpCallRestTimeInSeconds { get; set; }
}