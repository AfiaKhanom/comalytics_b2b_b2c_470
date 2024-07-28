using Nop.Core;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;

namespace NopStation.Plugin.Misc.B2B.SysproIntegration;

/// <summary>
/// Represents the SysproIntegration plugin
/// </summary>
public class SysproIntegrationPlugin : BasePlugin, IMiscPlugin
{
    #region Fields

    private readonly ILocalizationService _localizationService;
    private readonly ISettingService _settingService;
    private readonly IWebHelper _webHelper;

    #endregion

    #region Ctor

    public SysproIntegrationPlugin(
        ILocalizationService localizationService,
        ISettingService settingService,
        IWebHelper webHelper)
    {
        _localizationService = localizationService;
        _settingService = settingService;
        _webHelper = webHelper;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets a configuration page URL
    /// </summary>
    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/SysproIntegration/Configure";
    }

    /// <summary>
    /// Install the plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task InstallAsync()
    {
        //settings
        await _settingService.SaveSettingAsync(new SysproIntegrationSettings
        {
            BaseUrl = string.Empty,
            Token = string.Empty,
            HttpCallMaxRetries = 5,
            HttpCallRestTimeInMinutes = 1,
        });


        //locales
        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["NopStation.Plugin.Misc.B2B.SysproIntegration.Fields.BaseUrl"] = "Base Url",
            ["NopStation.Plugin.Misc.B2B.SysproIntegration.Fields.BaseUrl.Hint"] = "Base url of syspro",
            ["NopStation.Plugin.Misc.B2B.SysproIntegration.Fields.Token"] = "Token",
            ["NopStation.Plugin.Misc.B2B.SysproIntegration.Fields.Token.Hint"] = "Token for auth in syspro",
            ["NopStation.Plugin.Misc.B2B.SysproIntegration.General"] = "Connection details",
            ["NopStation.Plugin.Misc.B2B.SysproIntegration.Fields.HttpCallMaxRetries"] = "Max retries",
            ["NopStation.Plugin.Misc.B2B.SysproIntegration.Fields.HttpCallMaxRetries.Hint"] = "Max retries to connect with syspro",
            ["NopStation.Plugin.Misc.B2B.SysproIntegration.Fields.HttpCallRestTimeInMinutes"] = "Delay between each retry",
            ["NopStation.Plugin.Misc.B2B.SysproIntegration.Fields.HttpCallRestTimeInMinutes.Hint"] = "Delay between each retry in minutes"
        });

        await base.InstallAsync();
    }

    /// <summary>
    /// Uninstall the plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task UninstallAsync()
    {
        //settings
        await _settingService.DeleteSettingAsync<SysproIntegrationSettings>();

        //locales
        await _localizationService.DeleteLocaleResourcesAsync("NopStation.Plugin.Misc.B2B.SysproIntegration");

        await base.UninstallAsync();
    }

    #endregion
}