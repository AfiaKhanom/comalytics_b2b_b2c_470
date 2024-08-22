using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.ErpInterface;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.Misc.B2B.SysproIntegration;

/// <summary>
/// Represents the SysproIntegration plugin
/// </summary>
public class SysproIntegrationPlugin : BasePlugin, IAdminMenuPlugin, IErpIntegrationPlugin, IMiscPlugin, INopStationPlugin
{
    #region Fields

    private readonly ILocalizationService _localizationService;
    private readonly ISettingService _settingService;
    private readonly IWebHelper _webHelper;
    private readonly SysproIntegration.Services.IErpAccountService _erpAccountService;
    private const string THIRD_PARTY_PLUGINS = "Third party plugins";
    private const string PLUGIN_SYSTEM_NAME = "Misc.B2B.SysproIntegration";
    private const string PLUGIN_TITLE = "Syspro Integration";
    private const string PLUGIN_ICON_CLASS = "nav-icon fas fa-cube";
    private const bool PLUGIN_VISIBLE = true;
    private const string CHILD_NODE_CONFIG_SYSTEM_NAME = "Misc.B2B.SysproIntegration.Configuration";
    private const string CHILD_NODE_CONFIG_TITLE = "Configuration";
    private const string CHILD_NODE_CONFIG_CONTROLLER_NAME = "SysproIntegration";
    private const string CHILD_NODE_CONFIG_ACTION_NAME = "Configure";
    private const string CHILD_NODE_CONFIG_ICON_CLASS = "nav-icon fas fa-cogs";
    private const bool CHILD_NODE_CONFIG_VISIBLE = true;

    #endregion

    #region Ctor

    public SysproIntegrationPlugin(
        ILocalizationService localizationService,
        ISettingService settingService,
        IWebHelper webHelper,
        SysproIntegration.Services.IErpAccountService erpAccountService)
    {
        _localizationService = localizationService;
        _settingService = settingService;
        _webHelper = webHelper;
        _erpAccountService = erpAccountService;
    }

    #endregion

    #region Methods

    #region common

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
        var keyValuePairs = PluginResouces().ToDictionary(kv => kv.Key, kv => kv.Value);
        foreach (var keyValuePair in keyValuePairs)
        {
            await _localizationService.AddOrUpdateLocaleResourceAsync(keyValuePair.Key, keyValuePair.Value);
        }

        //settings
        await _settingService.SaveSettingAsync(new SysproIntegrationSettings
        {
            BaseUrl = string.Empty,
            Token = string.Empty,
            HttpCallMaxRetries = 5,
            HttpCallRestTimeInSeconds = 1,
        });

        await base.InstallAsync();
    }

    public async Task ManageSiteMapAsync(SiteMapNode rootNode)
    {
        var pluginNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == THIRD_PARTY_PLUGINS);

        if (pluginNode is null)
        {
            return;
        }

        pluginNode.ChildNodes.Add(new()
        {
            SystemName = PLUGIN_SYSTEM_NAME,
            Title = PLUGIN_TITLE,
            IconClass = PLUGIN_ICON_CLASS,
            Visible = PLUGIN_VISIBLE,
            ChildNodes = new List<SiteMapNode>() {
                    new()
                    {
                        SystemName = CHILD_NODE_CONFIG_SYSTEM_NAME,
                        Title = CHILD_NODE_CONFIG_TITLE,
                        ControllerName = CHILD_NODE_CONFIG_CONTROLLER_NAME,
                        ActionName = CHILD_NODE_CONFIG_ACTION_NAME,
                        IconClass = CHILD_NODE_CONFIG_ICON_CLASS,
                        Visible = CHILD_NODE_CONFIG_VISIBLE
                    }
                }
        });
    }

    public List<KeyValuePair<string, string>> PluginResouces()
    {
        var resources = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("NopStation.Plugin.Misc.B2B.SysproIntegration.Fields.BaseUrl", "Base Url"),
            new KeyValuePair<string, string>("NopStation.Plugin.Misc.B2B.SysproIntegration.Fields.BaseUrl.Hint", "Base url of syspro"),
            new KeyValuePair<string, string>("NopStation.Plugin.Misc.B2B.SysproIntegration.Fields.Token", "Token"),
            new KeyValuePair<string, string>("NopStation.Plugin.Misc.B2B.SysproIntegration.Fields.Token.Hint", "Token for auth in syspro"),
            new KeyValuePair<string, string>("NopStation.Plugin.Misc.B2B.SysproIntegration.General", "Connection details"),
            new KeyValuePair<string, string>("NopStation.Plugin.Misc.B2B.SysproIntegration.Fields.HttpCallMaxRetries", "Max retries"),
            new KeyValuePair<string, string>("NopStation.Plugin.Misc.B2B.SysproIntegration.Fields.HttpCallMaxRetries.Hint", "Max retries to connect with syspro"),
            new KeyValuePair<string, string>("NopStation.Plugin.Misc.B2B.SysproIntegration.Fields.HttpCallRestTimeInSeconds", "Delay between each retry"),
            new KeyValuePair<string, string>("NopStation.Plugin.Misc.B2B.SysproIntegration.Fields.HttpCallRestTimeInSeconds.Hint", "Delay between each retry in seconds")
        };

        return resources;
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

    #region erpaccount

    public async Task<ErpResponseModel> CreateAccountNoErpAsync(ErpCreateAccountModel erpCreateAccountModel)
    {
        throw new NotImplementedException();
    }

    public async Task<ErpResponseData<ErpAccountDataModel>> GetAccountFromErpAsync(ErpGetRequestModel erpRequest)
    {
        return await _erpAccountService.GetAccountFromErpAsync(erpRequest);
    }

    public async Task<ErpResponseData<IList<ErpAccountDataModel>>> GetAccountsFromErpAsync(ErpGetRequestModel erpRequest)
    {
        return await _erpAccountService.GetAccountsFromErpAsync(erpRequest);
    }

    #endregion

    #region order and quote

    public async Task<ErpResponseModel> CreateOrderOnErpAsync(ErpPlaceOrderDataModel erpRequest)
    {
        throw new NotImplementedException();
    }

    public async Task<ErpResponseData<IList<ErpPlaceOrderDataModel>>> GetOrderByAccountFromErpAsync(ErpGetRequestModel erpRequest)
    {
        throw new NotImplementedException();
    }

    public async Task<ErpResponseData<IList<ErpPlaceOrderDataModel>>> GetOrderByOrderNumberFromErpAsync(ErpGetRequestModel erpRequest)
    {
        throw new NotImplementedException();
    }

    public async Task<ErpResponseData<IList<ErpPlaceOrderDataModel>>> GetQuoteByAccountFromErpAsync(ErpGetRequestModel erpRequest)
    {
        throw new NotImplementedException();
    }

    public async Task<ErpResponseData<IList<ErpPlaceOrderDataModel>>> GetQuoteByQuoteNumberFromErpAsync(ErpGetRequestModel erpRequest)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region invoice
    public async Task<ErpResponseData<IList<ErpInvoiceDataModel>>> GetInvoiceByAccountNoFromErpAsync(ErpGetRequestModel erpRequest)
    {
        throw new NotImplementedException();
    }

    public async Task<ErpResponseData<string>> GetInvoicePdfByteCodeByDocumentNoFromErpAsync(ErpGetRequestModel erpRequest)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region product

    public async Task<ErpResponseData<ErpProductDataModel>> GetProductByItemNoFromErpAsync(ErpGetRequestModel erpRequest)
    {
        throw new NotImplementedException();
    }
    public async Task<ErpResponseData<IList<ErpProductDataModel>>> GetProductsFromErpAsync(ErpGetRequestModel erpRequest)
    {
        throw new NotImplementedException();
    }

    public Task ProductListLiveStockDataAsync(ErpAccount erpAccount, IList<Product> products, IProductService productService)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region pricing

    public async Task<ErpResponseData<ErpPriceGroupPricingDataModel>> GetProductGroupPriceFromErpAsync(ErpGetRequestModel erpRequest)
    {
        throw new NotImplementedException();
    }

    public async Task<ErpResponseData<IList<ErpPriceGroupPricingDataModel>>> GetProductGroupPricesFromErpAsync(ErpGetRequestModel erpRequest)
    {
        throw new NotImplementedException();
    }

    public async Task<ErpResponseData<ErpPriceSpecialPricingDataModel>> GetProductSpecialPriceFromErpAsync(ErpGetRequestModel erpRequest)
    {
        throw new NotImplementedException();
    }

    public Task<ErpResponseData<IList<ErpPriceSpecialPricingDataModel>>> GetProductSpecialPricesFromErpAsync(ErpGetRequestModel erpRequest)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region salesorg and warehouse

    public async Task<ErpResponseModel> GetSalesOrgsFromErpAsync(ErpGetRequestModel erpRequest)
    {
        throw new NotImplementedException();
    }

    public async Task<ErpResponseModel> GetSalesWarehouseFromErpAsync(ErpGetRequestModel erpRequest)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region stock

    public async Task<ErpResponseData<ErpProductDataModel>> GetStockByItemNoFromErpAsync(ErpGetRequestModel erpRequest)
    {
        throw new NotImplementedException();
    }

    public async Task<ErpResponseData<IList<ErpProductDataModel>>> GetStocksFromErpAsync(ErpGetRequestModel erpRequest)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region ship to address

    public async Task<ErpResponseData<IList<ErpShipToAddressDataModel>>> GetShipToAddressByAccountNumberFromErpAsync(ErpGetRequestModel erpRequest)
    {
        throw new NotImplementedException();
    }

    public async Task<string> GetSalesOrgCodeFromIntegrationSettings()
    {
        return string.Empty;
    }

    Task<ErpResponseData<ErpStockDataModel>> IErpIntegrationProductService.GetStockByItemNoFromErpAsync(ErpGetRequestModel erpRequest)
    {
        throw new NotImplementedException();
    }

    Task<ErpResponseData<IList<ErpStockDataModel>>> IErpIntegrationProductService.GetStocksFromErpAsync(ErpGetRequestModel erpRequest)
    {
        throw new NotImplementedException();
    }

    #endregion

    #endregion
}