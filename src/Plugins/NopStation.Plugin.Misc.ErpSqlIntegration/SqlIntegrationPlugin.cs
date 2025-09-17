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
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Misc.ErpSqlIntegration.Services;

namespace NopStation.Plugin.Misc.ErpSqlIntegration;

/// <summary>
/// Represents the SqlIntegration plugin
/// </summary>
public class SqlIntegrationPlugin : BasePlugin, IAdminMenuPlugin, IErpIntegrationPlugin, IMiscPlugin, INopStationPlugin
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
    private readonly IErpOrderService _erpOrderService;
    private readonly IB2BInvoiceService _b2BInvoiceService;
    private const string THIRD_PARTY_PLUGINS = "Third party plugins";
    private const string PLUGIN_SYSTEM_NAME = "Misc.B2B.SqlIntegration";
    private const string PLUGIN_TITLE = "Sql Integration";
    private const string PLUGIN_ICON_CLASS = "nav-icon fas fa-cube";
    private const bool PLUGIN_VISIBLE = true;
    private const string CHILD_NODE_CONFIG_SYSTEM_NAME = "Misc.B2B.SqlIntegration.Configuration";
    private const string CHILD_NODE_CONFIG_TITLE = "Configuration";
    private const string CHILD_NODE_CONFIG_ICON_CLASS = "nav-icon fas fa-cogs";
    private const bool CHILD_NODE_CONFIG_VISIBLE = true;

    #endregion

    #region Ctor
    
    public SqlIntegrationPlugin(
        ILocalizationService localizationService,
        ISettingService settingService,
        IWebHelper webHelper,
        IB2BAccountService b2BAccountService,
        IB2BProductService b2BProductService,
        IB2BPricingService b2BPricingService,
        IB2BStockService b2BStockService,
        IShipToAddressService shipToAddressService,
        IErpOrderService erpOrderService,
        IB2BInvoiceService b2BInvoiceService)
    {
        _localizationService = localizationService;
        _settingService = settingService;
        _webHelper = webHelper;
        _b2BAccountService = b2BAccountService;
        _b2BProductService = b2BProductService;
        _b2BPricingService = b2BPricingService;
        _b2BStockService = b2BStockService;
        _shipToAddressService = shipToAddressService;
        _erpOrderService = erpOrderService;
        _b2BInvoiceService = b2BInvoiceService;
    }

    #endregion

    #region Methods

    #region common

    /// <summary>
    /// Gets a configuration page URL
    /// </summary>
    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/SqlQueryTemplate/Configure";
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
        await _settingService.SaveSettingAsync(new SqlIntegrationSettings
        {
            HttpCallMaxRetries = 5,
            HttpCallRestTimeInSeconds = 1
        });

        await base.InstallAsync();
    }

    public override async Task UpdateAsync(string currentVersion, string targetVersion)
    {
        var keyValuePairs = PluginResouces().ToDictionary(kv => kv.Key, kv => kv.Value);
        foreach (var keyValuePair in keyValuePairs)
        {
            await _localizationService.AddOrUpdateLocaleResourceAsync(keyValuePair.Key, keyValuePair.Value);
        }

        await base.UpdateAsync(currentVersion, targetVersion);
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
                    ControllerName = "ErpSqlIntegration",
                    ActionName = "Configure",
                    IconClass = CHILD_NODE_CONFIG_ICON_CLASS,
                    Visible = CHILD_NODE_CONFIG_VISIBLE
                },
                new()
                {
                    SystemName = "Misc.B2B.SqlIntegration.SqlQueryTemplates",
                    Title = "Sql Query Templates",
                    ControllerName = "SqlQueryTemplate",
                    ActionName = "SqlQueryTemplates",
                    IconClass = "fas fa-database",
                    Visible = CHILD_NODE_CONFIG_VISIBLE
                },
                new()
                {
                    SystemName = "Misc.B2B.SqlIntegration.MappingOrder",
                    Title =  await _localizationService.GetResourceAsync("Misc.B2B.SqlIntegration.MappingOrder.Title"),
                    ControllerName = "ErpSqlIntegration",
                    ActionName = "ConfigurePlaceOrderParams",
                    IconClass = "nav-icon fas fa-cogs",
                    Visible = CHILD_NODE_CONFIG_VISIBLE
                }
            }
        });
    }

    public List<KeyValuePair<string, string>> PluginResouces()
    {
        var resources = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("NopStation.Plugin.Misc.ErpSqlIntegration.General", "Connection details"),
            new KeyValuePair<string, string>("NopStation.Plugin.Misc.ErpSqlIntegration.Fields.HttpCallMaxRetries", "Max retries"),
            new KeyValuePair<string, string>("NopStation.Plugin.Misc.ErpSqlIntegration.Fields.HttpCallMaxRetries.Hint", "Max retries to connect with Sql"),
            new KeyValuePair<string, string>("NopStation.Plugin.Misc.ErpSqlIntegration.Fields.HttpCallRestTimeInSeconds", "Delay between each retry"),
            new KeyValuePair<string, string>("NopStation.Plugin.Misc.ErpSqlIntegration.Fields.HttpCallRestTimeInSeconds.Hint", "Delay between each retry in seconds"),

            new KeyValuePair<string, string>("Admin.SqlQueryTemplate.Fields.Query.Required", "The query field is required"),
            new KeyValuePair<string, string>("Admin.SqlQueryTemplate.Fields.ErpSyncLevelId.ShouldMoreThanZero", "Sync Level ID must be greater than zero"),
            new KeyValuePair<string, string>("Admin.ErpSqlIntegration.SqlQueryTemplate.Updated", "SQL Query Template updated successfully"),
            new KeyValuePair<string, string>("Admin.ErpSqlIntegration.SqlQueryTemplate.Added", "SQL Query Template added successfully"),
            new KeyValuePair<string, string>("Admin.ErpSqlIntegration.SqlQueryTemplate.AlreadyExistForThisSyncLevel", "A SQL Query Template already exists for this sync level"),

            new KeyValuePair<string, string>("Plugin.Misc.NopStation.ErpSqlIntegration.Admin.SqlQueryTemplate.Title", "SQL Query Template"),
            new KeyValuePair<string, string>("Plugin.Misc.NopStation.ErpSqlIntegration.Admin.SqlQueryTemplates", "ERP SQL Integration - SQL Query Templates"),
            new KeyValuePair<string, string>("Plugin.Misc.NopStation.ErpSqlIntegration.Admin.SqlQueryTemplateModel.ErpSyncStatus", "ERP Sync Status"),
            new KeyValuePair<string, string>("Plugin.Misc.NopStation.ErpSqlIntegration.Admin.SqlQueryTemplateModel.Query", "Query"),

            new KeyValuePair<string, string>("Misc.B2B.SqlIntegration.SqlQueryTemplate.Edit", "Edit"),
            new KeyValuePair<string, string>("Admin.SqlQueryTemplate.EditSqlQueryTemplateDetails", "SQL Query Template Details"),
            new KeyValuePair<string, string>("Admin.SqlQueryTemplate.AddNew", "Add New"),
            new KeyValuePair<string, string>("Admin.SqlQueryTemplate.BackToList", "Back to List"),
            new KeyValuePair<string, string>("NopStation.Plugin.Misc.ErpSqlIntegration.Admin.General", "ERP SQL Integration General Settings"),
            new KeyValuePair<string, string>("Admin.SqlQueryTemplate.Info", "SQL Query Template Information"),

            new KeyValuePair<string, string>("NopStation.Plugin.Misc.ErpSqlIntegration.Fields.ConnectionString", "Connection String"),
            new KeyValuePair<string, string>("NopStation.Plugin.Misc.ErpSqlIntegration.Fields.ConnectionString.Hint", "Database connection string for ERP SQL Integration"),

            new KeyValuePair<string, string>("NopStation.Plugin.Misc.ErpSqlIntegration.Admin.SQLQueryTemplateModel.Query", "Query"),
            new KeyValuePair<string, string>("NopStation.Plugin.Misc.ErpSqlIntegration.Admin.SQLQueryTemplateModel.Query.Hint", "The SQL query to be executed for ERP integration"),

            new KeyValuePair<string, string>("NopStation.Plugin.Misc.ErpSqlIntegration.Admin.SQLQueryTemplateModel.ErpSyncLevel", "ERP Sync Level"),
            new KeyValuePair<string, string>("NopStation.Plugin.Misc.ErpSqlIntegration.Admin.SQLQueryTemplateModel.ErpSyncLevel.Hint", "The sync level for ERP SQL integration"),

            new KeyValuePair<string, string>("NopStation.Plugin.Misc.ErpSqlIntegration.Fields.BaseUrl", "Base URL"),
            new KeyValuePair<string, string>("NopStation.Plugin.Misc.ErpSqlIntegration.Fields.BaseUrl.Hint", "The base URL for the ERP SQL integration API"),

            new KeyValuePair<string, string>("NopStation.Plugin.Misc.ErpSqlIntegration.Fields.AuthUserName", "Auth Username"),
            new KeyValuePair<string, string>("NopStation.Plugin.Misc.ErpSqlIntegration.Fields.AuthUserName.Hint", "The username used for authenticating with the ERP SQL integration API"),

            new KeyValuePair<string, string>("NopStation.Plugin.Misc.ErpSqlIntegration.Fields.AuthPassword", "Auth Password"),
            new KeyValuePair<string, string>("NopStation.Plugin.Misc.ErpSqlIntegration.Fields.AuthPassword.Hint", "The password used for authenticating with the ERP SQL integration API"),

            // AdditionalHardcodedValueModel
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.AdditionalHardcodedValueModel.Fields.AdditionalHardCodedValues", "Additional Hardcoded Values"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.AdditionalHardcodedValueModel.Fields.AdditionalHardCodedValues.Hint", "Enter any additional fixed values that should be included with the req data."),

            // ErpOrderItemDataSettingsModel
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderItemDataSettings.Fields.Sku", "SKU"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderItemDataSettings.Fields.Sku.Hint", "The product's stock keeping unit."),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderItemDataSettings.Fields.BatchCode", "Batch Code"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderItemDataSettings.Fields.BatchCode.Hint", "Code identifying the product's manufacturing batch."),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderItemDataSettings.Fields.Description", "Description"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderItemDataSettings.Fields.Description.Hint", "A brief description of the product."),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderItemDataSettings.Fields.Quantity", "Quantity"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderItemDataSettings.Fields.Quantity.Hint", "The number of units ordered."),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderItemDataSettings.Fields.UnitOfMeasure", "Unit of Measure"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderItemDataSettings.Fields.UnitOfMeasure.Hint", "The unit used to measure the product (e.g., piece, box)."),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderItemDataSettings.Fields.SpecialInstruction", "Special Instruction"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderItemDataSettings.Fields.SpecialInstruction.Hint", "Any special instructions related to this item."),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderItemDataSettings.Fields.UnitPriceExclTax", "Unit Price (Excl. Tax)"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderItemDataSettings.Fields.UnitPriceExclTax.Hint", "Price per unit excluding tax."),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderItemDataSettings.Fields.UnitPriceInclTax", "Unit Price (Incl. Tax)"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderItemDataSettings.Fields.UnitPriceInclTax.Hint", "Price per unit including tax."),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderItemDataSettings.Fields.DiscountPercentage", "Discount Percentage"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderItemDataSettings.Fields.DiscountPercentage.Hint", "The discount percentage applied to the item."),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderItemDataSettings.Fields.DiscountAmountInclTax", "Discount Amount (Incl. Tax)"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderItemDataSettings.Fields.DiscountAmountInclTax.Hint", "The discount amount including tax."),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderItemDataSettings.Fields.DiscountAmountExclTax", "Discount Amount (Excl. Tax)"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderItemDataSettings.Fields.DiscountAmountExclTax.Hint", "The discount amount excluding tax."),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderItemDataSettings.Fields.PriceExclTax", "Total Price (Excl. Tax)"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderItemDataSettings.Fields.PriceExclTax.Hint", "Total price of the item excluding tax."),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderItemDataSettings.Fields.PriceInclTax", "Total Price (Incl. Tax)"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderItemDataSettings.Fields.PriceInclTax.Hint", "Total price of the item including tax."),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpGetRequestSettingsModel.Fields.ErpOrderPayloadLinesKey", "ERP Order Payload Lines Key"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpGetRequestSettingsModel.Fields.ErpOrderPayloadLinesKey.Hint", "The key in the ERP request payload used to identify the order line items."),

            // ErpOrderSettingsModel
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.AccountNumber", "Account Number"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.AccountNumber.Hint", "The ERP account number for the customer."),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.Location", "Location"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.Location.Hint", "The location associated with the order."),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.CustomOrderNumber", "Custom Order Number"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.CustomOrderNumber.Hint", "A custom identifier for the order."),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.RepCode", "Representative Code"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.RepCode.Hint", "Code for the sales representative handling the order."),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.AddressCode", "Address Code"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.AddressCode.Hint", "Code referencing the delivery address."),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.Notes", "Order Notes"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.Notes.Hint", "Any notes related to the order."),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.CustomerReference", "Customer Reference"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.CustomerReference.Hint", "The customer's internal reference for the order."),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.DeliveryInstruction", "Delivery Instructions"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.DeliveryInstruction.Hint", "Special instructions for delivering the order."),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.DeliveryMethod", "Delivery Method"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.DeliveryMethod.Hint", "Method chosen for order delivery."),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.CustomerEmail", "Customer Email"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.CustomerEmail.Hint", "Email address of the customer."),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.OrderType", "Order Type"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.OrderType.Hint", "The type of order (e.g., standard, rush)."),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.OrderTax", "Order Tax"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.OrderTax.Hint", "Total tax applied to the order."),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.OrderSubtotalExclTax", "Order Subtotal (Excl. Tax)"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.OrderSubtotalExclTax.Hint", "Subtotal of the order excluding tax."),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.OrderSubtotalInclTax", "Order Subtotal (Incl. Tax)"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.OrderSubtotalInclTax.Hint", "Subtotal of the order including tax."),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.OrderDate", "Order Date"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.OrderDate.Hint", "The date the order was placed."),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.DeliveryDate", "Delivery Date"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.DeliveryDate.Hint", "The expected delivery date."),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.DateRequired", "Date Required"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.DateRequired.Hint", "The required date for the order."),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.ShippingAmount", "Shipping Amount"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.ShippingAmount.Hint", "Total shipping cost for the order."),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.ErpOrderPayloadRootKey", "ERP Order Payload Root Key"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.ErpOrderPayloadRootKey.Hint", "The root key in the ERP payload for order data."),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpGetRequestSettingsModel.Fields.WarehouseCode", "WarehouseCode"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpGetRequestSettingsModel.Fields.WarehouseCode.Hint", "The WarehouseCode of item."),

            new KeyValuePair<string, string>("Misc.B2B.SqlIntegration.MappingOrder.Title", "Mappings for order placement params"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.MappingOrder.Title", "Mappings for order placement params"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.MappingOrder.Settings", "Order mapping settings"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.MappingOrderItem.Settings", "Order item mapping settings"),

            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.MappingOrderItem.DuplicateKey", "Mappings could not be saved because the following keys are duplicated: {0}"),

            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpShippingAddressPayloadSettings.Fields.Name", "Name"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpShippingAddressPayloadSettings.Fields.Name.Hint", "Specify the name key for the ERP req payload."),

            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpShippingAddressPayloadSettings.Fields.Email", "Email"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpShippingAddressPayloadSettings.Fields.Email.Hint", "Specify the email key for the ERP req payload."),

            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpShippingAddressPayloadSettings.Fields.Company", "Company"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpShippingAddressPayloadSettings.Fields.Company.Hint", "Specify the company key for the ERP req payload."),

            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpShippingAddressPayloadSettings.Fields.Address1", "Address 1"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpShippingAddressPayloadSettings.Fields.Address1.Hint", "Specify the address 1 key for the ERP req payload."),

            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpShippingAddressPayloadSettings.Fields.Address2", "Address 2"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpShippingAddressPayloadSettings.Fields.Address2.Hint", "Specify the address 2 key for the ERP req payload."),

            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpShippingAddressPayloadSettings.Fields.Address3", "Address 3"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpShippingAddressPayloadSettings.Fields.Address3.Hint", "Specify the address 3 key for the ERP req payload."),

            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpShippingAddressPayloadSettings.Fields.City", "City"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpShippingAddressPayloadSettings.Fields.City.Hint", "Specify the city key for the ERP req payload."),

            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpShippingAddressPayloadSettings.Fields.StateProvince", "State/Province"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpShippingAddressPayloadSettings.Fields.StateProvince.Hint", "Specify the state/province key for the ERP req payload."),

            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpShippingAddressPayloadSettings.Fields.Region", "Region"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpShippingAddressPayloadSettings.Fields.Region.Hint", "Specify the region key for the ERP req payload."),

            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpShippingAddressPayloadSettings.Fields.ZipPostalCode", "Zip/Postal Code"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpShippingAddressPayloadSettings.Fields.ZipPostalCode.Hint", "Specify the zip/postal code key for the ERP req payload."),

            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpShippingAddressPayloadSettings.Fields.Country", "Country"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpShippingAddressPayloadSettings.Fields.Country.Hint", "Specify the country key for the ERP req payload."),

            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpShippingAddressPayloadSettings.Fields.PhoneNumber", "Phone Number"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpShippingAddressPayloadSettings.Fields.PhoneNumber.Hint", "Specify the phone number key for the ERP req payload."),

            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpShippingAddressPayloadSettings.Fields.Suburb", "Suburb"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpShippingAddressPayloadSettings.Fields.Suburb.Hint", "Specify the suburb key for the ERP req payload."),

            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpShippingAddressPayloadSettings.Fields.ShippingAddressPayloadKey", "Shipping address payload key"),
            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.ErpShippingAddressPayloadSettings.Fields.ShippingAddressPayloadKey.Hint", "Specify the JSON key for the ERP req payload that contains the shipping address block."),

            new KeyValuePair<string, string>("Plugins.NopStation.Misc.ErpSqlIntegration.MappingShippingAddress.Settings", "Shipping Address mapping settings"),

            new (
                "Integration.IntegrationSecretKey.Hint",
                "Enter any 16-character key (letters and digits only) to represent your token generation."
            ),
            new (
                "Integration.IntegrationSecretKey",
                "Integration Token"
            ),
            new (
                "Integration.Response.TokenExpired",
                "Token Expired"
            ),
            new (
                "Integration.Response.InvalidToken",
                "Invalid Token"
            ),
            new (
                "Integration.Login.CustomerRole",
                "Please check if the user have Administration role"
            ),
            new (
                "Integration.Login.Permission.Denied",
                "User don't have administration role"
            ),
            new (
                "Integration.Login.InvalidIntegrationSecretKey",
                "Integration SecretKey Is Not Set."
            ),
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
        await _settingService.DeleteSettingAsync<SqlIntegrationSettings>();

        //locales
        await _localizationService.DeleteLocaleResourcesAsync("NopStation.Plugin.Misc.ErpSqlIntegration");

        await _localizationService.DeleteLocaleResourceAsync(
            "Integration.IntegrationSecretKey.Hint"
        );

        await _localizationService.DeleteLocaleResourceAsync(
            "Integration.IntegrationSecretKey"
        );

        await _localizationService.DeleteLocaleResourceAsync(
            "Integration.Response.TokenExpired"
        );

        await _localizationService.DeleteLocaleResourceAsync(
            "Integration.Response.InvalidToken"
        );

        await _localizationService.DeleteLocaleResourceAsync(
            "Integration.Login.CustomerRole"
        );

        await _localizationService.DeleteLocaleResourceAsync(
            "Integration.Login.Permission.Denied"
        );
        await _localizationService.DeleteLocaleResourceAsync(
            "Integration.Login.InvalidIntegrationSecretKey"
        );

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

    public async Task<ErpResponseData<IList<ErpAccountDataModel>>> GetAllAccountCreditFromErpAsync(ErpGetRequestModel erpRequest)
    {
        return await _b2BAccountService.GetAllAccountCreditFromErpAsync(erpRequest);
    }

    #endregion

    #region order and quote

    public async Task<ErpResponseModel> CreateOrderOnErpAsync(ErpPlaceOrderDataModel erpRequest)
    {
        return await _erpOrderService.CreateOrderOnErpAsync(erpRequest);
    }

    public async Task<ErpResponseData<IList<ErpPlaceOrderDataModel>>> GetOrderByAccountFromErpAsync(ErpGetRequestModel erpRequest)
    {
        return await _erpOrderService.GetOrderByAccountFromErpAsync(erpRequest);
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
        return await _b2BInvoiceService.GetInvoiceByAccountNoFromErpAsync(erpRequest);
    }

    public async Task<ErpResponseData<string>> GetInvoicePdfByteCodeByDocumentNoFromErpAsync(ErpGetRequestModel erpRequest)
    {
        return await _b2BInvoiceService.GetInvoicePdfByteCodeByDocumentNoFromErpAsync(erpRequest);
    }

    public async Task<ErpResponseData<string>> GetStatementPdfByteCodeFromErpAsync(ErpGetRequestModel erpRequest)
    {
        return await _b2BInvoiceService.GetStatementPdfByteCodeFromErpAsync(erpRequest);
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
        var groupProductPricings = await _b2BPricingService.GetProductGroupPricingFromErpAsync(erpRequest);
        var response = new ErpResponseData<ErpPriceGroupPricingDataModel>
        {
            ErpResponseModel = new ErpResponseModel
            {
                IsError = groupProductPricings.ErpResponseModel.IsError,
                StatusCode = groupProductPricings.ErpResponseModel.StatusCode,
                ErrorShortMessage = groupProductPricings.ErpResponseModel.ErrorShortMessage,
                ErrorFullMessage = groupProductPricings.ErpResponseModel.ErrorFullMessage,
            }
        };

        if (groupProductPricings.Data != null && groupProductPricings.Data.Any())
        {
            var pricing = groupProductPricings.Data.FirstOrDefault();
            response.Data = pricing;
        }
        else
        {
            response.Data = new ErpPriceGroupPricingDataModel();
        }

        return response;
    }

    public async Task<ErpResponseData<IList<ErpPriceGroupPricingDataModel>>> GetProductGroupPricesFromErpAsync(ErpGetRequestModel erpRequest)
    {
        return await _b2BPricingService.GetProductGroupPricingFromErpAsync(erpRequest);
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

    public async Task<ErpResponseData<IList<ErpShipToAddressDataModel>>> GetShipToAddressesFromErpAsync(ErpGetRequestModel erpRequest)
    {
        return await _shipToAddressService.GetShipToAddressFromErpAsync(erpRequest);
    }

    #endregion

    #endregion
}