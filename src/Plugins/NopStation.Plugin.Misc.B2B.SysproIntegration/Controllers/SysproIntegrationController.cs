using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.B2B.SysproIntegration.Models;

namespace NopStation.Plugin.Misc.B2B.SysproIntegration.Controllers;

[AutoValidateAntiforgeryToken]
public class SysproIntegrationController : BasePluginController
{

    #region Fields

    private readonly IStoreContext _storeContext;
    private readonly ISettingService _settingService;
    private readonly INotificationService _notificationService;
    private readonly ILocalizationService _localizationService;

    #endregion

    #region Ctor

    public SysproIntegrationController(IStoreContext storeContext,
        ISettingService settingService,
        INotificationService notificationService,
        ILocalizationService localizationService)
    {
        _storeContext = storeContext;
        _settingService = settingService;
        _notificationService = notificationService;
        _localizationService = localizationService;
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Prepare ConfigurationModel
    /// </summary>
    /// <param name="model">Model</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    protected async Task PrepareModelAsync(ConfigurationModel model)
    {
        //load settings for active store scope
        var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var sysproIntegrationSettings = await _settingService.LoadSettingAsync<SysproIntegrationSettings>(storeId);


        model.BaseUrl = sysproIntegrationSettings.BaseUrl;
        model.Token = sysproIntegrationSettings.Token;
        model.HttpCallMaxRetries = sysproIntegrationSettings.HttpCallMaxRetries;
        model.HttpCallRestTimeInMinutes = sysproIntegrationSettings.HttpCallRestTimeInMinutes;
    }

    #endregion

    #region Methods

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    public async Task<IActionResult> Configure()
    {
        var model = new ConfigurationModel();
        await PrepareModelAsync(model);

        return View("~/Plugins/Misc.B2B.SysproIntegration/Views/Configure.cshtml", model);
    }

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [HttpPost, ActionName("Configure")]
    [FormValueRequired("save")]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!ModelState.IsValid)
            return await Configure();

        var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var sysproIntegrationSettings = await _settingService.LoadSettingAsync<SysproIntegrationSettings>(storeId);

        //set API key
        sysproIntegrationSettings.BaseUrl = model.BaseUrl;
        sysproIntegrationSettings.Token = model.Token;
        sysproIntegrationSettings.HttpCallMaxRetries = model.HttpCallMaxRetries;
        sysproIntegrationSettings.HttpCallRestTimeInMinutes = model.HttpCallRestTimeInMinutes;

        await _settingService.SaveSettingAsync(sysproIntegrationSettings, settings => settings.BaseUrl, storeId, clearCache: false);
        await _settingService.SaveSettingAsync(sysproIntegrationSettings, settings => settings.Token, storeId, clearCache: false);
        await _settingService.SaveSettingAsync(sysproIntegrationSettings, settings => settings.HttpCallMaxRetries, storeId, clearCache: false);
        await _settingService.SaveSettingAsync(sysproIntegrationSettings, settings => settings.HttpCallRestTimeInMinutes, storeId, clearCache: false);
        await _settingService.ClearCacheAsync();

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return await Configure();
    }

    #endregion
}