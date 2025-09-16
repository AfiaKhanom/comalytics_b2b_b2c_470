using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Controllers;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.ErpSqlIntegration.Areas.Admin.Factories;
using NopStation.Plugin.Misc.ErpSqlIntegration.Areas.Admin.Models;
using NopStation.Plugin.Misc.ErpSqlIntegration.Models;
using NopStation.Plugin.Misc.ErpSqlIntegration.Services;
using NopStation.Plugin.Misc.ErpSqlIntegration.Services.SqLQueryTemplates;
using NopStation.Plugin.Misc.ErpSqlIntegration.Settings;

namespace NopStation.Plugin.Misc.ErpSqlIntegration.Areas.Admin.Controllers;

public class ErpSqlIntegrationController : NopStationAdminController
{
    #region Fields

    private readonly IErpActivityLogsService _erpActivityLogsService;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly IPermissionService _permissionService;
    private readonly INopFileProvider _fileProvider;
    private readonly ILogger _logger;
    private readonly ISqlQueryTemplateModelFactory _sqlQueryTemplateModelFactory;
    private readonly ISqlQueryTemplatService _sqlQueryTemplatService;
    private readonly SqlClient _sqlClient;
    private readonly IStoreContext _storeContext;
    private readonly ISettingService _settingService;
    private readonly ErpPlaceOrderSettings _erpPlaceOrderSettings;
    private readonly ErpPlaceOrderItemSettings _erpPlaceOrderItemSettings;
    private readonly ErpShippingAddressPayloadSettings _erpShippingAddressPayloadSettings;
    private readonly ISqlIntegrationService _sqlIntegrationService;

    #endregion Fields

    #region Ctor

    public ErpSqlIntegrationController(IErpActivityLogsService erpActivityLogsService,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IPermissionService permissionService,
        INopFileProvider nopFileProvider,
        ILogger logger,
        ISqlQueryTemplateModelFactory sqlQueryTemplateModelFactory,
        ISqlQueryTemplatService sqlQueryTemplatService,
        SqlClient sqlClient,
        IStoreContext storeContext,
        ISettingService settingService,
        ErpPlaceOrderSettings erpPlaceOrderSettings,
        ErpPlaceOrderItemSettings erpPlaceOrderItemSettings,
        ISqlIntegrationService sqlIntegrationService,
        ErpShippingAddressPayloadSettings erpShippingAddressPayloadSettings)
    {
        _erpActivityLogsService = erpActivityLogsService;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _permissionService = permissionService;
        _fileProvider = nopFileProvider;
        _logger = logger;
        _sqlQueryTemplateModelFactory = sqlQueryTemplateModelFactory;
        _sqlQueryTemplatService = sqlQueryTemplatService;
        _sqlClient = sqlClient;
        _storeContext = storeContext;
        _settingService = settingService;
        _erpPlaceOrderSettings = erpPlaceOrderSettings;
        _erpPlaceOrderItemSettings = erpPlaceOrderItemSettings;
        _sqlIntegrationService = sqlIntegrationService;
        _erpShippingAddressPayloadSettings = erpShippingAddressPayloadSettings;
    }

    #endregion Ctor

    #region Configuration

    private List<string> GetKeysFromHardCodedValues(string jsonString)
    {
        var items = JsonConvert.DeserializeObject<List<AdditionalValues>>(jsonString);

        return items?
            .Where(item => item != null && !string.IsNullOrWhiteSpace(item.Key))
            .Select(item => item.Key.Trim())
            .ToList() ?? new List<string>();
    }

    private (bool, string) IsDuplicateKeysPresent<TModel>(TModel model, string property = "", bool checkOnlyAdditionalHardCodedValues = false)
    {
        if (model == null)
        {
            return (false, string.Empty);
        }

        List<string> keys = null;

        var hardCodedProp = typeof(TModel).GetProperty(property);

        if (hardCodedProp != null && hardCodedProp.PropertyType == typeof(string))
        {
            var additionalValues = hardCodedProp.GetValue(model) as string;

            if (!string.IsNullOrWhiteSpace(additionalValues))
            {
                keys = GetKeysFromHardCodedValues(additionalValues);
            }
        }

        if (keys is null)
            keys = new List<string>();

        // Check other string properties
        if (!checkOnlyAdditionalHardCodedValues)
        {
            var properties = typeof(TModel).GetProperties();
            foreach (var prop in properties)
            {
                if (prop.Name == property)
                    continue;

                if (prop.PropertyType == typeof(string))
                {
                    var value = prop.GetValue(model) as string;
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        keys.Add(value.Trim());
                    }
                }
            }
        }

        var duplicateKeys = keys
            .GroupBy(k => k)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        return (duplicateKeys.Any(), string.Join(", ", duplicateKeys));
    }

    protected async Task PrepareModelAsync(ConfigurationModel model)
    {
        //load settings for active store scope
        var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var sqlIntegrationSettings = await _settingService.LoadSettingAsync<SqlIntegrationSettings>(storeId);

        model.HttpCallMaxRetries = sqlIntegrationSettings.HttpCallMaxRetries;
        model.HttpCallRestTimeInSeconds = sqlIntegrationSettings.HttpCallRestTimeInSeconds;
        model.ConnectionString = sqlIntegrationSettings.ConnectionString;
        model.BaseUrl = sqlIntegrationSettings.BaseUrl;
        model.AuthUserName = sqlIntegrationSettings.AuthUserName;
        model.AuthPassword = sqlIntegrationSettings.AuthPassword;
        model.IntegrationSecretKey = sqlIntegrationSettings.IntegrationSecretKey;
    }

    public async Task<IActionResult> Configure()
    {
        var model = new ConfigurationModel();
        await PrepareModelAsync(model);

        return View(model);
    }

    [FormValueRequired("save")]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!ModelState.IsValid)
            return await Configure();

        var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var sqlIntegrationSettings = await _settingService.LoadSettingAsync<SqlIntegrationSettings>(storeId);

        sqlIntegrationSettings.HttpCallMaxRetries = model.HttpCallMaxRetries;
        sqlIntegrationSettings.HttpCallRestTimeInSeconds = model.HttpCallRestTimeInSeconds;
        sqlIntegrationSettings.ConnectionString = model.ConnectionString;
        sqlIntegrationSettings.BaseUrl = model.BaseUrl;
        sqlIntegrationSettings.AuthPassword = model.AuthPassword;
        sqlIntegrationSettings.AuthUserName = model.AuthUserName;
        sqlIntegrationSettings.IntegrationSecretKey = model.IntegrationSecretKey;

        await _settingService.SaveSettingAsync(sqlIntegrationSettings, settings => settings.HttpCallMaxRetries, storeId, clearCache: false);
        await _settingService.SaveSettingAsync(sqlIntegrationSettings, settings => settings.HttpCallRestTimeInSeconds, storeId, clearCache: false);
        await _settingService.SaveSettingAsync(sqlIntegrationSettings, settings => settings.ConnectionString, storeId, clearCache: false);
        await _settingService.SaveSettingAsync(sqlIntegrationSettings, settings => settings.BaseUrl, storeId, clearCache: false);
        await _settingService.SaveSettingAsync(sqlIntegrationSettings, settings => settings.AuthPassword, storeId, clearCache: false);
        await _settingService.SaveSettingAsync(sqlIntegrationSettings, settings => settings.AuthUserName, storeId, clearCache: false);
        await _settingService.SaveSettingAsync(sqlIntegrationSettings, settings => settings.IntegrationSecretKey, storeId, clearCache: false);
        await _settingService.ClearCacheAsync();

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return await Configure();
    }

    public async Task<IActionResult> ConfigurePlaceOrderParams()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();

        var model = _erpPlaceOrderSettings.ToSettingsModel<ErpOrderSettingsModel>();
        model.ErpOrderItemDataSettingsModel = _erpPlaceOrderItemSettings.ToSettingsModel<ErpOrderItemDataSettingsModel>();
        model.ErpShippingAddressPayloadSettingsModel = _erpShippingAddressPayloadSettings.ToSettingsModel<ErpShippingAddressPayloadSettingsModel>();
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ConfigurePlaceOrderParams(ErpOrderSettingsModel model)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var (hasDuplicates, duplicateKeys) = IsDuplicateKeysPresent(model, nameof(ErpOrderSettingsModel.AdditionalHardCodedValues), false);
        if (hasDuplicates)
        {
            _notificationService.ErrorNotification(string.Format(await _localizationService.GetResourceAsync("Plugins.NopStation.Misc.ErpSqlIntegration.MappingOrderItem.DuplicateKey"), duplicateKeys));
            return View(model);
        }

        (hasDuplicates, duplicateKeys) = IsDuplicateKeysPresent(model.ErpOrderItemDataSettingsModel, nameof(ErpOrderItemDataSettingsModel.AdditionalHardCodedValues), false);
        if (hasDuplicates)
        {
            _notificationService.ErrorNotification(string.Format(await _localizationService.GetResourceAsync("Plugins.NopStation.Misc.ErpSqlIntegration.MappingOrderItem.DuplicateKey"), duplicateKeys));
            return View(model);
        }

        (hasDuplicates, duplicateKeys) = IsDuplicateKeysPresent(model.ErpShippingAddressPayloadSettingsModel, nameof(ErpShippingAddressPayloadSettingsModel.AdditionalHardCodedValues), false);
        if (hasDuplicates)
        {
            _notificationService.ErrorNotification(string.Format(await _localizationService.GetResourceAsync("Plugins.NopStation.Misc.ErpSqlIntegration.MappingOrderItem.DuplicateKey"), duplicateKeys));
            return View(model);
        }

        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        await _settingService.SaveSettingAsync(model.ToSettings(_erpPlaceOrderSettings), storeScope);
        await _settingService.SaveSettingAsync(model.ErpOrderItemDataSettingsModel.ToSettings(_erpPlaceOrderItemSettings), storeScope);
        await _settingService.SaveSettingAsync(model.ErpShippingAddressPayloadSettingsModel.ToSettings(_erpShippingAddressPayloadSettings), storeScope);
        await _settingService.ClearCacheAsync();

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.NopStation.Misc.ErpSqlIntegration.MappingPlaceOrder.SavedSuccessfully"));

        return await ConfigurePlaceOrderParams();
    }

    #endregion Configuration
}