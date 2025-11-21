using System.Linq.Expressions;
using System.Reflection;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using NopStation.Plugin.B2B.ERPIntegrationCore;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.Misc.B2B.ODataIntegration.Areas.Admin.Factories;
using NopStation.Plugin.Misc.B2B.ODataIntegration.Areas.Admin.Model;
using NopStation.Plugin.Misc.B2B.ODataIntegration.Areas.Admin.Models;
using NopStation.Plugin.Misc.B2B.ODataIntegration.GetRequestSettings;
using NopStation.Plugin.Misc.B2B.ODataIntegration.Services;
using NopStation.Plugin.Misc.B2B.ODataIntegration.Settings;
using NopStation.Plugin.Misc.B2B.ODataIntegration.Settings.PlaceOrderSettings;
using NopStation.Plugin.Misc.Core.Controllers;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.Areas.Admin.Controllers
{
    public class D365IntegrationController : NopStationAdminController
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly ICustomerService _customerService;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IErpLogsService _erpLogsService;
        private readonly IErpActivityLogsService _erpActivityLogsService;
        private readonly ErpShipToAddressSettings _erpShipToAddressSettings;
        private readonly ErpStockSettings _erpStockSettings;
        private readonly ErpOrderSettings _erpOrderSettings;
        private readonly ErpOrderItemDataSettings _erpOrderItemDataSettings;
        private readonly ErpOrderShippingAddressSettings _erpOrderShippingAddressSettings;
        private readonly ErpOrderBillingAddressSettings _erpOrderBillingAddressSettings;
        private readonly ErpInvoiceDataSettings _erpInvoiceDataSettings;
        private readonly ErpPriceSpecialPricingDataSettings _erpPriceSpecialPricingDataSettings;
        private readonly ErpPriceGroupPricingDataSettings _erpPriceGroupPricingDataSettings;
        private readonly ErpProductSettings _erpProductSettings;
        private readonly ErpCategoryDataSettings _erpCategoryDataSettings;
        private readonly ErpAccountGetRequestSettings _erpAccountGetRequestSettings;
        private readonly ErpProductGetRequestSettings _erpProductGetRequestSettings;
        private readonly ErpShipToAddressGetRequestSettings _erpShipToAddressGetRequestSettings;
        private readonly ErpStockGetRequestSettings _erpStockGetRequestSettings;
        private readonly ErpOrderGetRequestSettings _erpOrderGetRequestSettings;
        private readonly ErpSpecialPriceGetRequestSettings _erpSpecialPriceGetRequestSettings;
        private readonly ErpInvoiceGetRequestSettings _erpInvoiceGetRequestSettings;
        private readonly ErpGroupPriceGetRequestSettings _erpGroupPriceGetRequestSettings;
        private readonly ErpInvoicePdfSettings _erpInvoicePdfSettings;
        private readonly ErpInvoicePdfGetRequestSettings _erpInvoicePdfGetRequestSettings;
        private readonly ErpPlaceOrderSettings _erpPlaceOrderSettings;
        private readonly ErpPlaceOrderShippingAddressSettings _erpPlaceOrderShippingAddressSettings;
        private readonly ErpPlaceOrderBillingAddressSettings _erpPlaceOrderBillingAddressSettings;
        private readonly ErpPlaceOrderItemSettings _erpPlaceOrderItemSettings;
        private readonly ID365Service _d365Service;
        private readonly IModelFactory _modelFactory;



        private const string EDIT_SETTINGS_SYSTEM_KEYWORD = "EditSettings";

        #endregion

        #region Ctor

        public D365IntegrationController(
            IWorkContext workContext,
            IStoreContext storeContext,
            ISettingService settingService,
            ICustomerService customerService,
            IPermissionService permissionService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IErpLogsService erpLogsService,
            IErpActivityLogsService erpActivityLogsService,
            ErpShipToAddressSettings erpShipToAddressSettings,
            ErpStockSettings erpStockSettings,
            ErpOrderSettings erpOrderSettings,
            ErpOrderItemDataSettings erpOrderItemDataSettings,
            ErpOrderBillingAddressSettings erpOrderBillingAddressSettings,
            ErpOrderShippingAddressSettings erpOrderShippingAddressSettings,
            ErpInvoiceDataSettings erpInvoiceDataSettings,
            ErpPriceSpecialPricingDataSettings erpPriceSpecialPricingDataSettings,
            ErpPriceGroupPricingDataSettings erpPriceGroupPricingDataSettings,
            ErpProductSettings erpProductSettings,
            ErpCategoryDataSettings erpCategoryDataSettings,
            ErpAccountGetRequestSettings erpAccountGetRequestSettings,
            ErpProductGetRequestSettings erpProductGetRequestSettings,
            ErpShipToAddressGetRequestSettings erpShipToAddressGetRequestSettings,
            ErpStockGetRequestSettings erpStockGetRequestSettings,
            ErpOrderGetRequestSettings erpOrderGetRequestSettings,
            ErpSpecialPriceGetRequestSettings erpSpecialPriceGetRequestSettings,
            ErpInvoiceGetRequestSettings erpInvoiceGetRequestSettings,
            ErpGroupPriceGetRequestSettings erpGroupPriceGetRequestSettings,
            ErpInvoicePdfSettings erpInvoicePdfSettings,
            ErpInvoicePdfGetRequestSettings erpInvoicePdfGetRequestSettings,
            ErpPlaceOrderSettings erpPlaceOrderSettings,
            ErpPlaceOrderShippingAddressSettings erpPlaceOrderShippingAddressSettings,
            ErpPlaceOrderBillingAddressSettings erpPlaceOrderBillingAddressSettings,
            ErpPlaceOrderItemSettings erpPlaceOrderItemSettings,
            ID365Service d365Service,
            IModelFactory modelFactory)
        {
            _workContext = workContext;
            _storeContext = storeContext;
            _settingService = settingService;
            _customerService = customerService;
            _permissionService = permissionService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _erpLogsService = erpLogsService;
            _erpActivityLogsService = erpActivityLogsService;
            _erpShipToAddressSettings = erpShipToAddressSettings;
            _erpStockSettings = erpStockSettings;
            _erpOrderSettings = erpOrderSettings;
            _erpOrderItemDataSettings = erpOrderItemDataSettings;
            _erpOrderBillingAddressSettings = erpOrderBillingAddressSettings;
            _erpOrderShippingAddressSettings = erpOrderShippingAddressSettings;
            _erpInvoiceDataSettings = erpInvoiceDataSettings;
            _erpPriceSpecialPricingDataSettings = erpPriceSpecialPricingDataSettings;
            _erpPriceGroupPricingDataSettings = erpPriceGroupPricingDataSettings;
            _erpProductSettings = erpProductSettings;
            _erpCategoryDataSettings = erpCategoryDataSettings;
            _erpAccountGetRequestSettings = erpAccountGetRequestSettings;
            _erpProductGetRequestSettings = erpProductGetRequestSettings;
            _erpShipToAddressGetRequestSettings = erpShipToAddressGetRequestSettings;
            _erpStockGetRequestSettings = erpStockGetRequestSettings;
            _erpOrderGetRequestSettings = erpOrderGetRequestSettings;
            _erpSpecialPriceGetRequestSettings = erpSpecialPriceGetRequestSettings;
            _erpInvoiceGetRequestSettings = erpInvoiceGetRequestSettings;
            _erpGroupPriceGetRequestSettings = erpGroupPriceGetRequestSettings;
            _erpInvoicePdfSettings = erpInvoicePdfSettings;
            _erpInvoicePdfGetRequestSettings = erpInvoicePdfGetRequestSettings;
            _erpPlaceOrderSettings = erpPlaceOrderSettings;
            _erpPlaceOrderShippingAddressSettings = erpPlaceOrderShippingAddressSettings;
            _erpPlaceOrderBillingAddressSettings = erpPlaceOrderBillingAddressSettings;
            _erpPlaceOrderItemSettings = erpPlaceOrderItemSettings;
            _d365Service = d365Service;
            _modelFactory = modelFactory;
        }

        #endregion

        #region Utilities

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<D365IntegrationSettings>(storeScope);
            var model = settings.ToSettingsModel<ConfigurationModel>();

            model.ActiveStoreScopeConfiguration = storeScope;

            if (storeScope > 0)
            {
                model.ErpCallTimeOut_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.ErpCallTimeOut, storeScope);
                model.HttpCallMaxRetries_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.HttpCallMaxRetries, storeScope);
                model.HttpCallRestTimeInMinutes_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.HttpCallRestTimeInMinutes, storeScope);
                model.ShippingCostItemPayload_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.ShippingCostItemPayload, storeScope);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = model.ToSettings(await _settingService.LoadSettingAsync<D365IntegrationSettings>(storeScope));
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.ErpCallTimeOut, model.ErpCallTimeOut_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.HttpCallMaxRetries, model.HttpCallMaxRetries_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.HttpCallRestTimeInMinutes, model.HttpCallRestTimeInMinutes_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.AdditionalMappings, model.AdditionalMappings_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.DefaultDateTimeFormat, model.DefaultDateTimeFormat_OverrideForStore, storeScope, false);

            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.IntegrationSecretKey, model.IntegrationSecretKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.ShippingCostItemPayload, model.ShippingCostItemPayload_OverrideForStore, storeScope, false);

            if (!string.IsNullOrWhiteSpace(model.AdditionalMappings))
            {
                var result = SaveAdditionalMappings(model.AdditionalMappings);

                if (!result.Success)
                {
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync(result.ErrorMessage));
                }
            }

            if (!string.IsNullOrWhiteSpace(model.DefaultDateTimeFormat))
            {
                var result = SaveDefaultDateTimeFormat(model.DefaultDateTimeFormat);

                if (!result.Success)
                {
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync(result.ErrorMessage));
                }
            }
            await _settingService.ClearCacheAsync();

            await _erpActivityLogsService.InsertErpActivityAsync(EDIT_SETTINGS_SYSTEM_KEYWORD, await _localizationService.GetResourceAsync("Plugins.NopStation.D365Integration.ActivityLog.EditConfigurations"));

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return RedirectToAction("Configure");
        }

        public async Task<IActionResult> ConfigureApiUrls()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var apiUrlSetting = await _settingService.LoadSettingAsync<D365IntegrationApiUrlSettings>(storeScope);
            var model = apiUrlSetting.ToSettingsModel<D365IntegrationApiUrlSettingModel>();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ConfigureApiUrls(D365IntegrationApiUrlSettingModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var apiUrlSetting = await _settingService.LoadSettingAsync<D365IntegrationApiUrlSettings>(storeScope);
            var settings = model.ToSettings(apiUrlSetting);

            await _settingService.SaveSettingAsync(settings, storeScope);
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettings.SavedSuccessfully"));

            return RedirectToAction("ConfigureApiUrls");
        }

        public async Task<IActionResult> MappingAccount()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var accountSetting = await _settingService.LoadSettingAsync<ErpAccountSetting>(storeScope);

            var accountRequestSetting = await _settingService.LoadSettingAsync<ErpAccountGetRequestSettings>(storeScope);

            var model = accountSetting.ToSettingsModel<ErpAccountSettingModel>();
            model.ErpGetRequestSettingsModel = accountRequestSetting.ToSettingsModel<ErpGetRequestSettingsModel>();

            model.ExcludePropertySelectedValues = await _modelFactory.GetPropertiesToExcludeSelectedItemsAsync(s => s.AccountPropertiesToExclude);
            model.AvailableProperties = _modelFactory.GetPropertiesToExcludeSelectList(accountSetting, model);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> MappingAccount(ErpAccountSettingModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();
            
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();

            var (hasDuplicates, duplicateKeys) = IsDuplicateKeysPresent(model, nameof(ErpGetRequestSettingsModel.AdditionalFilters), true);
            if (hasDuplicates)
            {
                var errorMessage = string.Format(await _localizationService.GetResourceAsync("Plugins.NopStation.D365Integration.DuplicateKey"), duplicateKeys);
                _notificationService.ErrorNotification(errorMessage);
                return View(model);
            }

            var (result, errorMsg) = await SaveExcludePropertySettingsAsync(model.ExcludePropertySelectedValues, s => s.AccountPropertiesToExclude);

            if (!result)
            {
                _notificationService.ErrorNotification(errorMsg);
                return View(model);
            }


            #region save account settings

            var accountSetting = await _settingService.LoadSettingAsync<ErpAccountSetting>(storeScope);
            var settings = model.ToSettings(accountSetting);

            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.AccountNumber, model.AccountNumber_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.AccountName, model.AccountName_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.Address1, model.Address1_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.Address2, model.Address2_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.Address3, model.Address3_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.City, model.City_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.StateProvince, model.StateProvince_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.ZipPostalCode, model.ZipPostalCode_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.Country, model.Country_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.PhoneNumber, model.PhoneNumber_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.Email, model.Email_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.CompanyNo, model.CompanyNo_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.VatNumber, model.VatNumber_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.CreditLimit, model.CreditLimit_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.ErpSalesOrgCode, model.ErpSalesOrgCode_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.BillingSuburb, model.BillingSuburb_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.BillingName, model.BillingName_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.CreditLimitAvailable, model.CreditLimitAvailable_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.CurrentBalance, model.CurrentBalance_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.AllowOverspend, model.AllowOverspend_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.PreFilterFacets, model.PreFilterFacets_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.PaymentTypeCode, model.PaymentTypeCode_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.PriceGroupCode, model.PriceGroupCode_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.PercentageOfStockAllowed, model.PercentageOfStockAllowed_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.DeliveryRoute, model.DeliveryRoute_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.IsActive, model.IsActive_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.IsDeleted, model.IsDeleted_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.OverrideBackOrderingConfigSetting, model.OverrideBackOrderingConfigSetting_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.AllowAccountsBackOrdering, model.AllowAccountsBackOrdering_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.AllowAccountsAddressEditOnCheckout, model.AllowAccountsAddressEditOnCheckout_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.LoyaltyBalance, model.LoyaltyBalance_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.MinimumOrderValue, model.MinimumOrderValue_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.LoyaltyCardNumber, model.LoyaltyCardNumber_OverrideForStore, storeScope, false);


            #endregion

            await _settingService.ClearCacheAsync();
            #region save account get request settings

            var accountRequestSetting = await _settingService.LoadSettingAsync<ErpAccountGetRequestSettings>(storeScope);
            var requestSettings = model.ErpGetRequestSettingsModel.ToSettings(accountRequestSetting);
            var requestModel = model.ErpGetRequestSettingsModel;

            await _settingService.SaveSettingOverridablePerStoreAsync(requestSettings, x => x.AccountNumber, requestModel.AccountNumber_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(requestSettings, x => x.LastChangedDate, requestModel.LastChangedDate_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(requestSettings, x => x.Location, requestModel.Location_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(requestSettings, x => x.AccountSyncLimit, requestModel.AccountSyncLimit_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(requestSettings, x => x.AdditionalFilters, requestModel.AdditionalFilters_OverrideForStore, storeScope, false);

            #endregion

            await _settingService.ClearCacheAsync();
            
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.NopStation.D365Integration.ErpAccountSetting.SavedSuccessfully"));

            return RedirectToAction("MappingAccount");
        }

        public async Task<IActionResult> MappingProduct()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var model = _erpProductSettings.ToSettingsModel<ErpProductSettingModel>();
            var erpCategorySettings = await _settingService.LoadSettingAsync<ErpCategoryDataSettings>();
            var productRequestSetting = await _settingService.LoadSettingAsync<ErpProductGetRequestSettings>(storeScope);

            model.ErpCategoryDataSettingsModel = erpCategorySettings.ToSettingsModel<ErpCategoryDataSettingsModel>();
            model.ErpGetRequestSettingsModel = productRequestSetting.ToSettingsModel<ErpGetRequestSettingsModel>();

            model.ExcludePropertySelectedValues = await _modelFactory.GetPropertiesToExcludeSelectedItemsAsync(s => s.ProductPropertiesToExclude);
            model.AvailableProperties = _modelFactory.GetPropertiesToExcludeSelectList(_erpProductSettings, model);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> MappingProduct(ErpProductSettingModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (hasDuplicates, duplicateKeys) = IsDuplicateKeysPresent(model, nameof(ErpGetRequestSettingsModel.AdditionalFilters), true);
            if (hasDuplicates)
            {
                var errorMessage = string.Format(await _localizationService.GetResourceAsync("Plugins.NopStation.D365Integration.DuplicateKey"), duplicateKeys);
                _notificationService.ErrorNotification(errorMessage);
                return View(model);
            }

            var (result, errorMsg) = await SaveExcludePropertySettingsAsync(model.ExcludePropertySelectedValues, s => s.ProductPropertiesToExclude);

            if (!result)
            {
                _notificationService.ErrorNotification(errorMsg);
                return View(model);
            }

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();

            await _settingService.SaveSettingAsync(model.ToSettings(_erpProductSettings), storeScope);
            await _settingService.SaveSettingAsync(model.ErpCategoryDataSettingsModel.ToSettings(_erpCategoryDataSettings), storeScope);

            var productRequestSetting = await _settingService.LoadSettingAsync<ErpProductGetRequestSettings>(storeScope);
            var requestSettings = model.ErpGetRequestSettingsModel.ToSettings(productRequestSetting);
            await _settingService.SaveSettingAsync(requestSettings, storeScope);

            await _settingService.ClearCacheAsync();

            await _d365Service.LoadProductAttributeMappings();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.NopStation.D365Integration.ErpProductDataSettings.SavedSuccessfully"));

            return RedirectToAction("MappingProduct");
        }

        public async Task<IActionResult> MappingShipToAddress()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var model = _erpShipToAddressSettings.ToSettingsModel<ErpShipToAddressSettingModel>();
            var shipToAddressRequestSetting = await _settingService.LoadSettingAsync<ErpShipToAddressGetRequestSettings>();
            model.ErpGetRequestSettingsModel = shipToAddressRequestSetting.ToSettingsModel<ErpGetRequestSettingsModel>();


            model.ExcludePropertySelectedValues = await _modelFactory.GetPropertiesToExcludeSelectedItemsAsync(s => s.ShipToAddressPropertiesToExclude);
            model.AvailableProperties = _modelFactory.GetPropertiesToExcludeSelectList(_erpShipToAddressSettings, model);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> MappingShipToAddress(ErpShipToAddressSettingModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (hasDuplicates, duplicateKeys) = IsDuplicateKeysPresent(model, nameof(ErpGetRequestSettingsModel.AdditionalFilters), true);
            if (hasDuplicates)
            {
                var errorMessage = string.Format(await _localizationService.GetResourceAsync("Plugins.NopStation.D365Integration.DuplicateKey"), duplicateKeys);
                _notificationService.ErrorNotification(errorMessage);
                return View(model);
            }

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();

            var updatedSettings = model.ToSettings(_erpShipToAddressSettings);
            var shipToAddressRequestSetting = await _settingService.LoadSettingAsync<ErpShipToAddressGetRequestSettings>(storeScope);
            var requestSettings = model.ErpGetRequestSettingsModel.ToSettings(shipToAddressRequestSetting);

            var (result, errorMsg) = await SaveExcludePropertySettingsAsync(model.ExcludePropertySelectedValues,s => s.ShipToAddressPropertiesToExclude);

            if (!result)
            {
                _notificationService.ErrorNotification(errorMsg);
                return View(model);
            }

            await _settingService.SaveSettingAsync(requestSettings, storeScope);
            await _settingService.SaveSettingAsync(updatedSettings, storeScope);

            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.NopStation.D365Integration.ErpShipToAddressSettings.SavedSuccessfully"));

            return RedirectToAction("MappingShipToAddress");
        }

        public async Task<IActionResult> MappingStock()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var model = _erpStockSettings.ToSettingsModel<ErpStockSettingModel>();
            var stockRequestSetting = await _settingService.LoadSettingAsync<ErpStockGetRequestSettings>();
            model.ErpGetRequestSettingsModel = stockRequestSetting.ToSettingsModel<ErpGetRequestSettingsModel>();

            model.ExcludePropertySelectedValues = await _modelFactory.GetPropertiesToExcludeSelectedItemsAsync(s => s.StockPropertiesToExclude);
            model.AvailableProperties = _modelFactory.GetPropertiesToExcludeSelectList(_erpStockSettings, model);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> MappingStock(ErpStockSettingModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();

            var updatedSettings = model.ToSettings(_erpStockSettings);
            var stockRequestSetting = await _settingService.LoadSettingAsync<ErpStockGetRequestSettings>(storeScope);
            var requestSettings = model.ErpGetRequestSettingsModel.ToSettings(stockRequestSetting);

            await _settingService.SaveSettingAsync(requestSettings, storeScope);
            await _settingService.SaveSettingAsync(updatedSettings, storeScope);

            var (result, errorMsg) = await SaveExcludePropertySettingsAsync(model.ExcludePropertySelectedValues, s => s.StockPropertiesToExclude);

            if (!result)
            {
                _notificationService.ErrorNotification(errorMsg);
                return View(model);
            }

            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.NopStation.D365Integration.ErpStockSettings.SavedSuccessfully"));

            return RedirectToAction("MappingStock");
        }

        public async Task<IActionResult> MappingOrder()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var model = _erpOrderSettings.ToSettingsModel<ErpOrderSettingsModel>();
            var erpOrderItemDataSettings = await _settingService.LoadSettingAsync<ErpOrderItemDataSettings>();
            var erpShippingAddressSettings = await _settingService.LoadSettingAsync<ErpOrderShippingAddressSettings>();
            var erpBillingAddressSettings = await _settingService.LoadSettingAsync<ErpOrderBillingAddressSettings>();
            var orderRequestSetting = await _settingService.LoadSettingAsync<ErpOrderGetRequestSettings>();

            model.ErpOrderItemDataSettingsModel = erpOrderItemDataSettings.ToSettingsModel<ErpOrderItemDataSettingsModel>();
            model.ErpShippingAddressSettingsModel = erpShippingAddressSettings.ToSettingsModel<ErpShippingAddressModel>();
            model.ErpBillingAddressSettingsModel = erpBillingAddressSettings.ToSettingsModel<ErpBillingAddressModel>();
            model.ErpGetRequestSettingsModel = orderRequestSetting.ToSettingsModel<ErpGetRequestSettingsModel>();

            model.ExcludePropertySelectedValues = await _modelFactory.GetPropertiesToExcludeSelectedItemsAsync(s => s.OrderPropertiesToExclude);
            model.AvailableProperties = _modelFactory.GetPropertiesToExcludeSelectList(_erpOrderSettings, model);

            model.ErpOrderItemDataSettingsModel.ExcludePropertySelectedValues = await _modelFactory.GetPropertiesToExcludeSelectedItemsAsync(s => s.OrderItemPropertiesToExclude);
            model.ErpOrderItemDataSettingsModel.AvailableProperties = _modelFactory.GetPropertiesToExcludeSelectList(erpOrderItemDataSettings, model.ErpOrderItemDataSettingsModel);


            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> MappingOrder(ErpOrderSettingsModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (result, errorMsg) = await SaveExcludePropertySettingsAsync(model.ExcludePropertySelectedValues, s => s.OrderPropertiesToExclude);

            if (!result)
            {
                _notificationService.ErrorNotification(errorMsg);
                return View(model);
            }

            (result, errorMsg) = await SaveExcludePropertySettingsAsync(model.ErpOrderItemDataSettingsModel.ExcludePropertySelectedValues, s => s.OrderItemPropertiesToExclude);

            if (!result)
            {
                _notificationService.ErrorNotification(errorMsg);
                return View(model);
            }

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();

            await _settingService.SaveSettingAsync(model.ToSettings(_erpOrderSettings), storeScope);
            await _settingService.SaveSettingAsync(model.ErpOrderItemDataSettingsModel.ToSettings(_erpOrderItemDataSettings), storeScope);
            await _settingService.SaveSettingAsync(model.ErpShippingAddressSettingsModel .ToSettings(_erpOrderShippingAddressSettings), storeScope);
            await _settingService.SaveSettingAsync(model.ErpBillingAddressSettingsModel.ToSettings(_erpOrderBillingAddressSettings), storeScope);
            await _settingService.SaveSettingAsync(model.ErpGetRequestSettingsModel.ToSettings(_erpOrderGetRequestSettings), storeScope);
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.NopStation.D365Integration.ErpOrderSettings.SavedSuccessfully"));

            return RedirectToAction("MappingOrder");
        }

        public async Task<IActionResult> MappingInvoice()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var model = _erpInvoiceDataSettings.ToSettingsModel<ErpInvoiceDataSettingsModel>();

            model.ErpGetRequestSettingsModel = _erpInvoiceGetRequestSettings.ToSettingsModel<ErpGetRequestSettingsModel>();
            model.ExcludePropertySelectedValues = await _modelFactory.GetPropertiesToExcludeSelectedItemsAsync(s => s.InvoicePropertiesToExclude);
            model.AvailableProperties = _modelFactory.GetPropertiesToExcludeSelectList(_erpInvoiceDataSettings, model);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> MappingInvoice(ErpInvoiceDataSettingsModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (result, errorMsg) = await SaveExcludePropertySettingsAsync(model.ExcludePropertySelectedValues, s => s.InvoicePropertiesToExclude);

            if (!result)
            {
                _notificationService.ErrorNotification(errorMsg);
                return View(model);
            }

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();

            await _settingService.SaveSettingAsync(model.ToSettings(_erpInvoiceDataSettings), storeScope);
            await _settingService.SaveSettingAsync(model.ErpGetRequestSettingsModel.ToSettings(_erpInvoiceGetRequestSettings), storeScope);
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.NopStation.D365Integration.ErpInvoiceSettings.SavedSuccessfully"));

            return RedirectToAction("MappingInvoice");
        }

        public async Task<IActionResult> MappingInvoicePdf()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var model = _erpInvoicePdfSettings.ToSettingsModel<ErpInvoicePdfSettingsModel>();
            model.ErpGetRequestSettingsModel = _erpInvoicePdfGetRequestSettings.ToSettingsModel<ErpGetRequestSettingsModel>();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> MappingInvoicePdf(ErpInvoicePdfSettingsModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();

            await _settingService.SaveSettingAsync(model.ToSettings(_erpInvoicePdfSettings), storeScope);
            await _settingService.SaveSettingAsync(model.ErpGetRequestSettingsModel.ToSettings(_erpInvoicePdfGetRequestSettings), storeScope);
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.NopStation.D365Integration.ErpInvoicePdfSettings.SavedSuccessfully"));

            return RedirectToAction("MappingInvoicePdf");
        }

        public async Task<IActionResult> MappingSpecialPricing()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var model = _erpPriceSpecialPricingDataSettings.ToSettingsModel<ErpPriceSpecialPricingDataSettingsModel>();
            model.ErpGetRequestSettingsModel = _erpSpecialPriceGetRequestSettings.ToSettingsModel<ErpGetRequestSettingsModel>();

            model.ExcludePropertySelectedValues = await _modelFactory.GetPropertiesToExcludeSelectedItemsAsync(s => s.SpecialPricePropertiesToExclude);
            model.AvailableProperties = _modelFactory.GetPropertiesToExcludeSelectList(_erpPriceSpecialPricingDataSettings, model);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> MappingSpecialPricing(ErpPriceSpecialPricingDataSettingsModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (result, errorMsg) = await SaveExcludePropertySettingsAsync(model.ExcludePropertySelectedValues, s => s.SpecialPricePropertiesToExclude);

            if (!result)
            {
                _notificationService.ErrorNotification(errorMsg);
                return View(model);
            }

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();

            await _settingService.SaveSettingAsync(model.ToSettings(_erpPriceSpecialPricingDataSettings), storeScope);
            await _settingService.SaveSettingAsync(model.ErpGetRequestSettingsModel.ToSettings(_erpSpecialPriceGetRequestSettings), storeScope);
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.NopStation.D365Integration.ErpPriceSpecialPricingDataSettings.SavedSuccessfully"));

            return RedirectToAction("MappingSpecialPricing");
        }
        public async Task<IActionResult> MappingGroupPricing()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var model = _erpPriceGroupPricingDataSettings.ToSettingsModel<ErpPriceGroupPricingDataSettingsModel>();
            model.ErpGetRequestSettingsModel = _erpGroupPriceGetRequestSettings.ToSettingsModel<ErpGetRequestSettingsModel>();

            model.ExcludePropertySelectedValues = await _modelFactory.GetPropertiesToExcludeSelectedItemsAsync(s => s.GroupPricePropertiesToExclude);
            model.AvailableProperties = _modelFactory.GetPropertiesToExcludeSelectList(_erpPriceGroupPricingDataSettings, model);


            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> MappingGroupPricing(ErpPriceGroupPricingDataSettingsModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (result, errorMsg) = await SaveExcludePropertySettingsAsync(model.ExcludePropertySelectedValues, s => s.GroupPricePropertiesToExclude);

            if (!result)
            {
                _notificationService.ErrorNotification(errorMsg);
                return View(model);
            }

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            await _settingService.SaveSettingAsync(model.ToSettings(_erpPriceGroupPricingDataSettings), storeScope);
            await _settingService.SaveSettingAsync(model.ErpGetRequestSettingsModel.ToSettings(_erpGroupPriceGetRequestSettings), storeScope);
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.NopStation.D365Integration.ErpPriceGroupPricingDataSettings.SavedSuccessfully"));

            return RedirectToAction("MappingGroupPricing");
        }

        public async Task<IActionResult> MappingPlaceOrder()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();

            var erpPlaceOrderSettings = await _settingService.LoadSettingAsync<ErpPlaceOrderSettings>(storeScope);
            var erpPlaceOrderItemSettings = await _settingService.LoadSettingAsync<ErpPlaceOrderItemSettings>(storeScope);
            var erpPlaceOrderShippingAddressSettings = await _settingService.LoadSettingAsync<ErpPlaceOrderShippingAddressSettings>(storeScope);
            var erpPlaceOrderBillingAddressSettings = await _settingService.LoadSettingAsync<ErpPlaceOrderBillingAddressSettings>(storeScope);


            var model = erpPlaceOrderSettings.ToSettingsModel<ErpOrderSettingsModel>();
            model.ErpOrderItemDataSettingsModel = erpPlaceOrderItemSettings.ToSettingsModel<ErpOrderItemDataSettingsModel>();
            model.ErpShippingAddressSettingsModel = erpPlaceOrderShippingAddressSettings.ToSettingsModel<ErpShippingAddressModel>();
            model.ErpBillingAddressSettingsModel = erpPlaceOrderBillingAddressSettings.ToSettingsModel<ErpBillingAddressModel>();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> MappingPlaceOrder(ErpOrderSettingsModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (hasDuplicates, duplicateKeys) = IsDuplicateKeysPresent(model, nameof(ErpOrderSettingsModel.AdditionalHardCodedValues));
            if (hasDuplicates)
            {
                var errorMessage = string.Format(await _localizationService.GetResourceAsync("Plugins.NopStation.D365Integration.DuplicateKey"), duplicateKeys);
                _notificationService.ErrorNotification(errorMessage);
                return View(model);
            }

            (hasDuplicates, duplicateKeys) = IsDuplicateKeysPresent(model.ErpOrderItemDataSettingsModel, nameof(ErpOrderItemDataSettingsModel.AdditionalHardCodedValues));
            if (hasDuplicates)
            {
                var errorMessage = string.Format(await _localizationService.GetResourceAsync("Plugins.NopStation.D365Integration.DuplicateKey"), duplicateKeys);
                _notificationService.ErrorNotification(errorMessage);
                return View(model);
            }

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            await _settingService.SaveSettingAsync(model.ToSettings(_erpPlaceOrderSettings), storeScope);
            await _settingService.SaveSettingAsync(model.ErpOrderItemDataSettingsModel.ToSettings(_erpPlaceOrderItemSettings), storeScope);
            await _settingService.SaveSettingAsync(model.ErpShippingAddressSettingsModel.ToSettings(_erpPlaceOrderShippingAddressSettings), storeScope);
            await _settingService.SaveSettingAsync(model.ErpBillingAddressSettingsModel.ToSettings(_erpPlaceOrderBillingAddressSettings), storeScope);
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.NopStation.D365Integration.MappingPlaceOrder.SavedSuccessfully"));

            return RedirectToAction("MappingPlaceOrder");
        }

        public async Task<IActionResult> MappingCreateAccount()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var createAccountSettings = await _settingService.LoadSettingAsync<ErpCreateAccountSettings>(storeScope);
            var model = createAccountSettings.ToSettingsModel<ErpCreateAccountSettingsModel>();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> MappingCreateAccount(ErpCreateAccountSettingsModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var (hasDuplicates, duplicateKeys) = IsDuplicateKeysPresent(model, nameof(ErpCreateAccountSettingsModel.AdditionalHardCodedValues));
            if (hasDuplicates)
            {
                var errorMessage = string.Format(await _localizationService.GetResourceAsync("Plugins.NopStation.D365Integration.DuplicateKey"), duplicateKeys);
                _notificationService.ErrorNotification(errorMessage);
                return View(model);
            }

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var createAccountSettings = await _settingService.LoadSettingAsync<ErpCreateAccountSettings>(storeScope);

            var settings = model.ToSettings(createAccountSettings);

            await _settingService.SaveSettingAsync(settings, storeScope);
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.SavedSuccessfully"));

            return RedirectToAction("MappingCreateAccount");
        }

        public async Task<IActionResult> MappingCreateShipToAddress()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var createShipToAddressSettings = await _settingService.LoadSettingAsync<ErpCreateShipToAddressSettings>(storeScope);
            var model = createShipToAddressSettings.ToSettingsModel<ErpCreateShipToAddressSettingsModel>();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> MappingCreateShipToAddress(ErpCreateShipToAddressSettingsModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (hasDuplicates, duplicateKeys) = IsDuplicateKeysPresent(model, nameof(ErpCreateShipToAddressSettingsModel.AdditionalHardCodedValues));
            if (hasDuplicates)
            {
                var errorMessage = string.Format(await _localizationService.GetResourceAsync("Plugins.NopStation.D365Integration.DuplicateKey"), duplicateKeys);
                _notificationService.ErrorNotification(errorMessage);
                return View(model);
            }

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var createShipToAddressSettings = await _settingService.LoadSettingAsync<ErpCreateShipToAddressSettings>(storeScope);

            var settings = model.ToSettings(createShipToAddressSettings);

            await _settingService.SaveSettingAsync(settings, storeScope);
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.NopStation.D365Integration.ErpCreateShipToAddressSettingsModel.SavedSuccessfully"));

            return RedirectToAction("MappingCreateShipToAddress");
        }

        private (bool Success, string? ErrorMessage) SaveAdditionalMappings(string jsonString)
        {
            TranslationTableModel? mappings;
            try
            {
                mappings = JsonConvert.DeserializeObject<TranslationTableModel>(jsonString);
            }
            catch (JsonException)
            {
                return (false, "Invalid JSON format for translation table. Please provide values in proper JSON format.");
            }

            if(mappings.OrderTypes != null && mappings.OrderTypes.Any())
            {
                foreach (var item in mappings.OrderTypes)
                {
                    if (item == null)
                        continue;

                    if (!string.IsNullOrWhiteSpace(item.Key) && !string.IsNullOrWhiteSpace(item.Value))
                    {
                        _d365Service.AddOrUpdateAdditionalMapping(item.Key.Trim(), item.Value.Trim());
                    }
                }
            }

            if (mappings.DeliveryMethods != null && mappings.DeliveryMethods.Any())
            {
                foreach (var item in mappings.DeliveryMethods)
                {
                    if (item == null)
                        continue;

                    if (!string.IsNullOrWhiteSpace(item.Key) && !string.IsNullOrWhiteSpace(item.Value))
                    {
                        _d365Service.AddOrUpdateAdditionalMapping(item.Key.Trim(), item.Value.Trim());
                    }
                }
            }

            if (mappings.DocumentTypes != null && mappings.DocumentTypes.Any())
            {
                foreach (var item in mappings.DocumentTypes)
                {
                    if (item == null)
                        continue;

                    if (!string.IsNullOrWhiteSpace(item.Key) && !string.IsNullOrWhiteSpace(item.Value))
                    {
                        _d365Service.AddOrUpdateAdditionalMapping(item.Key.Trim(), item.Value.Trim());
                    }
                }
            }
            return (true, string.Empty);
        }

        private (bool Success, string? ErrorMessage) SaveDefaultDateTimeFormat(string format)
        {
            if (!string.IsNullOrWhiteSpace(format))
            {
                _d365Service.AddOrUpdateDefaultDateTimeFormat(format.Trim());
                return (true, string.Empty);
            }
            return (true, "DateTime format is empty.");
        }
        private List<string> GetKeysFromHardCodedValues(string jsonString)
        {
            var items = JsonConvert.DeserializeObject<List<AdditionalValues>>(jsonString);

            var keys = new List<string>();

            if (items == null)
                return keys;

            foreach (var item in items)
            {
                if (string.IsNullOrWhiteSpace(item?.Key))
                    continue;

                if (!string.IsNullOrWhiteSpace(item.Key))
                {
                    keys.Add(item.Key.Trim());
                }
            }
            return keys;
        }

        private (bool, string) IsDuplicateKeysPresent<TModel>(TModel model, string property = "", bool checkOnlyThisProperty = false)
        {
            if (model == null)
            {
                return (false, string.Empty);
            }

            var keys = new List<string>();

            // Get AdditionalHardCodedValues property
            // Get AdditionalFiltes property.

            var hardCodedProp = typeof(TModel).GetProperty(property);

            if (hardCodedProp != null && hardCodedProp.PropertyType == typeof(string))
            {
                var additionalValues = hardCodedProp.GetValue(model) as string;

                if (!string.IsNullOrWhiteSpace(additionalValues))
                {
                    keys = GetKeysFromHardCodedValues(additionalValues);
                }
            }
            // Get other string properties
            if (!checkOnlyThisProperty)
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

            bool hasDuplicates = duplicateKeys.Any();
            string duplicates = string.Join(", ", duplicateKeys);

            return (hasDuplicates, duplicates);
        }

        private async Task<(bool Success, string? ErrorMessage)> SaveExcludePropertySettingsAsync(
            IList<string> props,
            Expression<Func<ERPIntegrationCoreDataMappingSettings, string?>> propertySelector)
        {
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<ERPIntegrationCoreDataMappingSettings>(storeScope);

            var compiledSelector = propertySelector.Compile();

            if (propertySelector.Body is not MemberExpression memberExpr)
                return (false, "The selector must point to a property.");

            if (memberExpr.Member is not PropertyInfo propertyInfo)
                return (false, "The selector must point to a property.");

            propertyInfo.SetValue(settings, ConvertListToCommaSeparated(props));

            await _settingService.SaveSettingAsync(settings, storeScope);

            return (true, "");
        }

        private string ConvertListToCommaSeparated(IList<string> list)
        {
            if (list == null)
            {
                return string.Empty;
            }
            return string.Join(", ", list.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()));
        }

        #endregion
    }
}