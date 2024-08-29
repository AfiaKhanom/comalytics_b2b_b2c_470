using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.Misc.B2B.SysproIntegration.Services;
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
    private readonly IB2BAccountService _b2BAccountService;
    private readonly IB2BProductService _b2BProductService;
    private readonly IB2BPricingService _b2BPricingService;
    private readonly IB2BStockService _b2BStockService;
    private readonly IShipToAddressService _shipToAddressService;
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
        IB2BAccountService b2BAccountService,
        IB2BProductService b2BProductService,
        IB2BPricingService b2BPricingService,
        IB2BStockService b2BStockService,
        IShipToAddressService shipToAddressService)
    {
        _localizationService = localizationService;
        _settingService = settingService;
        _webHelper = webHelper;
        _b2BAccountService = b2BAccountService;
        _b2BProductService = b2BProductService;
        _b2BPricingService = b2BPricingService;
        _b2BStockService = b2BStockService;
        _shipToAddressService = shipToAddressService;
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
        var erpAccounts = await _b2BAccountService.GetAccountsFromErpAsync(erpRequest);
        var response = new ErpResponseData<ErpAccountDataModel>
        {
            ErpResponseModel = new ErpResponseModel
            {
                IsError = erpAccounts.ErpResponseModel.IsError,
                StatusCode = erpAccounts.ErpResponseModel.StatusCode,
                ErrorShortMessage = erpAccounts.ErpResponseModel.ErrorShortMessage,
                ErrorFullMessage = erpAccounts.ErpResponseModel.ErrorFullMessage,
            }
        };

        if (erpAccounts.Data != null && erpAccounts.Data.Any())
        {
            response.Data = erpAccounts.Data.FirstOrDefault();
        }
        else
        {
            response.Data = new ErpAccountDataModel();
        }

        return response;
    }

    public async Task<ErpResponseData<IList<ErpAccountDataModel>>> GetAccountsFromErpAsync(ErpGetRequestModel erpRequest)
    {
        return await _b2BAccountService.GetAccountsFromErpAsync(erpRequest);
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
        var erpProducts = await _b2BProductService.GetProductsFromErpAsync(erpRequest);
        var response = new ErpResponseData<ErpProductDataModel>
        {
            ErpResponseModel = new ErpResponseModel
            {
                IsError = erpProducts.ErpResponseModel.IsError,
                StatusCode = erpProducts.ErpResponseModel.StatusCode,
                ErrorShortMessage = erpProducts.ErpResponseModel.ErrorShortMessage,
                ErrorFullMessage = erpProducts.ErpResponseModel.ErrorFullMessage,
            }
        };

        if (erpProducts.Data != null && erpProducts.Data.Any())
        {
            response.Data = erpProducts.Data.FirstOrDefault();
        }
        else
        {
            response.Data = new ErpProductDataModel();
        }

        return response;
    }
    public async Task<ErpResponseData<IList<ErpProductDataModel>>> GetProductsFromErpAsync(ErpGetRequestModel erpRequest)
    {
        return await _b2BProductService.GetProductsFromErpAsync(erpRequest);
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
        var perAccountProductPricings = await _b2BPricingService.GetPerAccountProductPricingFromErpAsync(erpRequest);
        var response = new ErpResponseData<ErpPriceSpecialPricingDataModel>
        {
            ErpResponseModel = new ErpResponseModel
            {
                IsError = perAccountProductPricings.ErpResponseModel.IsError,
                StatusCode = perAccountProductPricings.ErpResponseModel.StatusCode,
                ErrorShortMessage = perAccountProductPricings.ErpResponseModel.ErrorShortMessage,
                ErrorFullMessage = perAccountProductPricings.ErpResponseModel.ErrorFullMessage,
            }
        };

        if (perAccountProductPricings.Data != null && perAccountProductPricings.Data.Any())
        {
            var pricing = perAccountProductPricings.Data.FirstOrDefault();
            response.Data = pricing;
        }
        else
        {
            response.Data = new ErpPriceSpecialPricingDataModel();
        }

        return response;
    }

    public async Task<ErpResponseData<IList<ErpPriceSpecialPricingDataModel>>> GetProductSpecialPricesFromErpAsync(ErpGetRequestModel erpRequest)
    {
        return await _b2BPricingService.GetPerAccountProductPricingFromErpAsync(erpRequest);
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

    public async Task<ErpResponseData<ErpStockDataModel>> GetStockByItemNoFromErpAsync(ErpGetRequestModel erpRequest)
    {
        var erpStock = await _b2BStockService.GetStockFromErpAsync(erpRequest);
        var response = new ErpResponseData<ErpStockDataModel>
        {
            ErpResponseModel = new ErpResponseModel
            {
                IsError = erpStock.ErpResponseModel.IsError,
                StatusCode = erpStock.ErpResponseModel.StatusCode,
                ErrorShortMessage = erpStock.ErpResponseModel.ErrorShortMessage,
                ErrorFullMessage = erpStock.ErpResponseModel.ErrorFullMessage,
            }
        };

        if (erpStock.Data != null && erpStock.Data.Any())
        {
            response.Data = erpStock.Data.FirstOrDefault();
        }
        else
        {
            response.Data = new ErpStockDataModel();
        }

        return response;
    }

    public async Task<ErpResponseData<IList<ErpStockDataModel>>> GetStocksFromErpAsync(ErpGetRequestModel erpRequest)
    {
        return await _b2BStockService.GetStockFromErpAsync(erpRequest);
    }

    #endregion

    #region ship to address

    public async Task<ErpResponseData<IList<ErpShipToAddressDataModel>>> GetShipToAddressByAccountNumberFromErpAsync(ErpGetRequestModel erpRequest)
    {
        return await _shipToAddressService.GetShipToAddressFromErpAsync(erpRequest);
    }

    public async Task<string> GetSalesOrgCodeFromIntegrationSettings()
    {
        return string.Empty;
    }

    #endregion

    #endregion
}