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
using NopStation.Plugin.Misc.B2B.ODataIntegration.Services;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration
{
    public class D365IntegrationPlugin : BasePlugin, IAdminMenuPlugin, IErpIntegrationPlugin, IMiscPlugin, INopStationPlugin
    {
        #region Fields
        private readonly ID365Service _d365Service;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly ILocalizationService _localizationService;



        private const string THIRD_PARTY_PLUGINS = "Third party plugins";
        private const string PLUGIN_SYSTEM_NAME = "NopStation.Plugin.Misc.B2B.ODataIntegration";
        private const string PLUGIN_TITLE = "BCS Integration";
        private const string PLUGIN_ICON_CLASS = "nav-icon fas fa-cube";
        private const bool PLUGIN_VISIBLE = true;
        private const string PLUGIN_VERSION = "1.00";
        private const string CHILD_NODE_CONFIG_SYSTEM_NAME = "NopStation.Plugin.Misc.B2B.ODataIntegration";
        private const string CHILD_NODE_CONFIG_TITLE = "Configuration";
        private const string CHILD_NODE_CONFIG_CONTROLLER_NAME = "D365Integration";
        private const string CHILD_NODE_CONFIG_ACTION_NAME = "Configure";
        private const string CHILD_NODE_CONFIG_ICON_CLASS = "nav-icon fas fa-cogs";
        private const bool CHILD_NODE_CONFIG_VISIBLE = true;
        private const string CHILD_NODE_MAPPING_ACCOUNT_SYSTEM_NAME = "NopStation.Plugin.Misc.B2B.ODataIntegration.MappingAccount";
        private const string CHILD_NODE_MAPPING_ACCOUNT_TITLE = "Plugins.NopStation.D365Integration.Admin.MappingAccount.Title";
        private const string CHILD_NODE_MAPPING_ACCOUNT_CONTROLLER_NAME = "D365Integration";
        private const string CHILD_NODE_MAPPING_ACCOUNT_ACTION_NAME = "MappingAccount";
        private const bool CHILD_NODE_MAPPING_ACCOUNT_VISIBLE = true;


        #endregion

        #region Ctor

        public D365IntegrationPlugin(
            ISettingService settingService,
            IWebHelper webHelper,
            ILocalizationService localizationService,
            ID365Service d365Service)

        {
            _settingService = settingService;
            _webHelper = webHelper;
            _localizationService = localizationService;
            _d365Service = d365Service;
        }

        #endregion

        #region Utils
        private Dictionary<string, string> GetLocaleResources()
        {
            return new Dictionary<string, string>
            {
                ["Plugins.NopStation.D365Integration.Configuration.Fields.InvoiceSyncLimit"] = "Invoice Sync Limit",
                ["Plugins.NopStation.D365Integration.Configuration.Fields.InvoiceSyncLimit.Hint"] = "Specify the number of Invoice Sync in each batch. Default is 100.",
                ["Plugins.NopStation.D365Integration.Configuration.Fields.AdditionalMappings"] = "Additional Mappings",["Plugins.NopStation.D365Integration.Configuration.Fields.AdditionalMappings.Hint"] = "Additional Mappings. This feild is for mapping the enums, specify the key and value.",
                ["Plugins.NopStation.D365Integration.ExcludePropertySelectedValues"] = "Skip Properties On Update",["Plugins.NopStation.D365Integration.ExcludePropertySelectedValues.Hint"] = "These properties will be skipped on update.",

                ["Plugins.NopStation.D365Integration.Configuration.Fields.DefaultDateTimeFormat"] = "Default DateTime Format",
                ["Plugins.NopStation.D365Integration.Configuration.Fields.DefaultDateTimeFormat.Hint"] = "Enter the date time format.",

                ["Plugins.NopStation.D365Integration.Configuration.Fields.ShippingCostItemPayload"] = "Shipping Cost Item Payload",
                ["Plugins.NopStation.D365Integration.Configuration.Fields.ShippingCostItemPayload.Hint"] = "This payload will be added while creating order at ERP.",


                #region API URL Settings
                ["Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettings.SystemName"] = "API URL Settings",
                ["Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettings.Title"] = "API URL Settings",
                ["Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettings.Settings"] = "API URL Settings",
                ["Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettings.SavedSuccessfully"] = "API URL settings have been saved successfully.",

                ["Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.AccessTokenUrl"] = "Access Token URL",
                ["Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.AccessTokenUrl.Hint"] = "Access Token URL",

                ["Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.ClientId"] = "Client Id",
                ["Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.ClientId.Hint"] = "Client Id",

                ["Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.ClientSecret"] = "Client Secret",
                ["Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.ClientSecret.Hint"] = "Client Secret",

                ["Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.Scope"] = "Scope",
                ["Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.Scope.Hint"] = "Scope",

                ["Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.ProductApiUrl"] = "Product API URL",
                ["Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.ProductApiUrl.Hint"] = "The endpoint URL for product data.",

                ["Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.AccountApiUrl"] = "Account API URL",
                ["Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.AccountApiUrl.Hint"] = "The endpoint URL for customer accounts.",

                ["Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.ShippingAddressApiUrl"] = "Shipping Address API URL",
                ["Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.ShippingAddressApiUrl.Hint"] = "The endpoint URL for shipping addresses.",

                ["Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.InvoiceApiUrl"] = "Invoice API URL",
                ["Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.InvoiceApiUrl.Hint"] = "The endpoint URL for invoice data.",

                ["Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.InvoicePdfApiUrl"] = "Invoice PDF API URL",
                ["Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.InvoicePdfApiUrl.Hint"] = "The endpoint URL for invoice PDF downloads.",

                ["Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.OrderApiUrl"] = "Order API URL",
                ["Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.OrderApiUrl.Hint"] = "The endpoint URL for orders.",

                ["Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.SpecialPriceApiUrl"] = "Special Price API URL",
                ["Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.SpecialPriceApiUrl.Hint"] = "The endpoint URL for special pricing.",

                ["Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.GroupPriceApiUrl"] = "Group Price API URL",
                ["Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.GroupPriceApiUrl.Hint"] = "The endpoint URL for group pricing.",

                ["Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.StockApiUrl"] = "Stock API URL",
                ["Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.StockApiUrl.Hint"] = "The endpoint URL for stock availability.",

                ["Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.CreateAccountApiUrl"] = "Create Account API URL",
                ["Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.CreateAccountApiUrl.Hint"] = "The endpoint URL for creating account.",

                ["Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.CreateShipToAddressApiUrl"] = "Create ShipToAddress API URL",
                ["Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.CreateShipToAddressApiUrl.Hint"] = "The endpoint URL for creating ShipToAddress.",

                ["Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.IsOrderItemExcluded"] = "Is Order Item Excluded For Placing Order.",
                ["Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.IsOrderItemExcluded.Hint"] = "Check to exclude order items from the placing order call.",

                #endregion

                #region Request Model
                ["Plugins.NopStation.D365Integration.ErpGetRequestSettings.SystemName"] = "ERP Get Request Settings",
                ["Plugins.NopStation.D365Integration.ErpGetRequestSettings.Title"] = "ERP Get Request Settings",
                ["Plugins.NopStation.D365Integration.ErpGetRequestSettings.Settings"] = "ERP Get Request Settings",
                ["Plugins.NopStation.D365Integration.ErpGetRequestSettings.SavedSuccessfully"] = "ERP Get Request Settings have been saved successfully.",

                ["Plugins.NopStation.D365Integration.ErpGetRequestSettingsModel.Fields.AccountNumber"] = "Account Number",
                ["Plugins.NopStation.D365Integration.ErpGetRequestSettingsModel.Fields.AccountNumber.Hint"] = "Specify the account number to filter the data.",

                ["Plugins.NopStation.D365Integration.ErpGetRequestSettingsModel.Fields.DocumentNumber"] = "Document Number",
                ["Plugins.NopStation.D365Integration.ErpGetRequestSettingsModel.Fields.DocumentNumber.Hint"] = "Specify the document number.",

                ["Plugins.NopStation.D365Integration.ErpGetRequestSettingsModel.Fields.OrderNumber"] = "Order Number",
                ["Plugins.NopStation.D365Integration.ErpGetRequestSettingsModel.Fields.OrderNumber.Hint"] = "Specify the order number.",

                ["Plugins.NopStation.D365Integration.ErpGetRequestSettingsModel.Fields.ProductSku"] = "Product SKU",
                ["Plugins.NopStation.D365Integration.ErpGetRequestSettingsModel.Fields.ProductSku.Hint"] = "Specify the product SKU.",

                ["Plugins.NopStation.D365Integration.ErpGetRequestSettingsModel.Fields.Start"] = "Start",
                ["Plugins.NopStation.D365Integration.ErpGetRequestSettingsModel.Fields.Start.Hint"] = "Starting index or record for data fetching.",

                ["Plugins.NopStation.D365Integration.ErpGetRequestSettingsModel.Fields.Limit"] = "Limit",
                ["Plugins.NopStation.D365Integration.ErpGetRequestSettingsModel.Fields.Limit.Hint"] = "Maximum number of records to fetch.",

                ["Plugins.NopStation.D365Integration.ErpGetRequestSettingsModel.Fields.LastChangedDate"] = "LastChangedDate",
                ["Plugins.NopStation.D365Integration.ErpGetRequestSettingsModel.Fields.LastChangedDate.Hint"] = "Filter records based on last changed date.",
                
                ["Plugins.NopStation.D365Integration.ErpGetRequestSettingsModel.Fields.DateFrom"] = "Date From",
                ["Plugins.NopStation.D365Integration.ErpGetRequestSettingsModel.Fields.DateFrom.Hint"] = "Filter records from this date.",
                
                ["Plugins.NopStation.D365Integration.ErpGetRequestSettingsModel.Fields.DateTo"] = "Date To",
                ["Plugins.NopStation.D365Integration.ErpGetRequestSettingsModel.Fields.DateTo.Hint"] = "Filter records till this date.",

                ["Plugins.NopStation.D365Integration.ErpGetRequestSettingsModel.Fields.SalesOrg"] = "SalesOrg",
                ["Plugins.NopStation.D365Integration.ErpGetRequestSettingsModel.Fields.SalesOrg.Hint"] = "Specify the salesOrg to filter.",
                
                ["Plugins.NopStation.D365Integration.ErpGetRequestSettingsModel.Fields.WarehouseCode"] = "WarehouseCode",
                ["Plugins.NopStation.D365Integration.ErpGetRequestSettingsModel.Fields.WarehouseCode.Hint"] = "Specify the warehouse code to filter.",

                ["Plugins.NopStation.D365Integration.ErpGetRequestSettingsModel.Fields.PriceCode"] = "Price Code",
                ["Plugins.NopStation.D365Integration.ErpGetRequestSettingsModel.Fields.PriceCode.Hint"] = "Specify the price code for filtering.",

                #endregion

                #region account mapping
                ["Plugins.NopStation.D365Integration.Admin.MappingAccount.Title"] = "Account Settings",
                ["Plugins.NopStation.D365Integration.Admin.MappingAccount.Settings"] = "Account Fields Mapping",

                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.AccountNumber"] = "Account Number",
                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.AccountNumber.Hint"] = "Account no",

                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.AccountName"] = "Account Name",
                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.AccountName.Hint"] = "Name of the account",

                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.ErpSalesOrgCode"] = "ERP Sales Org Code",
                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.ErpSalesOrgCode.Hint"] = "ERP sales organization code",

                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.BillingSuburb"] = "Billing Suburb",
                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.BillingSuburb.Hint"] = "Suburb of billing address",

                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.BillingName"] = "Billing Name",
                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.BillingName.Hint"] = "Name on the billing address",

                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.VatNumber"] = "VAT Number",
                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.VatNumber.Hint"] = "VAT registration number",

                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.CreditLimit"] = "Credit Limit",
                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.CreditLimit.Hint"] = "Total credit limit",

                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.CreditLimitUsed"] = "Credit Limit Used",
                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.CreditLimitUsed.Hint"] = "Used portion of credit limit",

                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.CreditLimitAvailable"] = "Credit Limit Available",
                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.CreditLimitAvailable.Hint"] = "Available credit limit",

                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.CurrentBalance"] = "Current Balance",
                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.CurrentBalance.Hint"] = "Current outstanding balance",

                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.AllowOverspend"] = "Allow Overspend",
                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.AllowOverspend.Hint"] = "Whether overspending is allowed",

                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.PreFilterFacets"] = "Pre-Filter Facets",
                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.PreFilterFacets.Hint"] = "Pre-applied filters or facets",

                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.PaymentTypeCode"] = "Payment Type Code",
                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.PaymentTypeCode.Hint"] = "Code indicating the payment type",

                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.PriceGroupCode"] = "Price Group Code",
                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.PriceGroupCode.Hint"] = "Code of the price group",

                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.CreditLimitStr"] = "Credit Limit (Formatted)",
                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.CreditLimitStr.Hint"] = "Formatted credit limit string",

                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.CreditLimitUsedStr"] = "Credit Limit Used (Formatted)",
                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.CreditLimitUsedStr.Hint"] = "Formatted used credit limit string",

                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.CreditLimitAvailableStr"] = "Credit Limit Available (Formatted)",
                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.CreditLimitAvailableStr.Hint"] = "Formatted available credit string",

                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.CurrentBalanceStr"] = "Current Balance (Formatted)",
                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.CurrentBalanceStr.Hint"] = "Formatted balance string",

                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.PercentageOfStockAllowed"] = "Percentage of Stock Allowed",
                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.PercentageOfStockAllowed.Hint"] = "Percentage of stock usage allowed",

                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.Address1"] = "Address 1",
                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.Address1.Hint"] = "First line of address",

                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.Address2"] = "Address 2",
                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.Address2.Hint"] = "Second line of address",

                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.Address3"] = "Address 3",
                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.Address3.Hint"] = "Third line of address (optional)",

                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.StateProvince"] = "State/Province",
                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.StateProvince.Hint"] = "State or province",

                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.City"] = "City",
                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.City.Hint"] = "City name",

                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.Country"] = "Country",
                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.Country.Hint"] = "Country name",

                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.ZipPostalCode"] = "Zip/Postal Code",
                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.ZipPostalCode.Hint"] = "Zip or postal code",

                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.PhoneNumber"] = "Phone Number",
                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.PhoneNumber.Hint"] = "Contact number",

                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.Email"] = "Email",
                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.Email.Hint"] = "Email address",

                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.DeliveryRoute"] = "Delivery Route",
                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.DeliveryRoute.Hint"] = "Delivery route code or name",

                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.CompanyNo"] = "Company No",
                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.CompanyNo.Hint"] = "Company registration number",

                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.IsActive"] = "Is Active",
                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.IsActive.Hint"] = "Whether account is active",

                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.IsDeleted"] = "Is Deleted",
                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.IsDeleted.Hint"] = "Whether account is deleted",

                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.UpdatedOnUtc"] = "Updated On (UTC)",
                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.UpdatedOnUtc.Hint"] = "Last updated datetime (UTC)",

                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.OverrideBackOrderingConfigSetting"] = "Override Backordering Config",
                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.OverrideBackOrderingConfigSetting.Hint"] = "Override global backordering setting",

                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.AllowAccountsBackOrdering"] = "Allow Backordering",
                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.AllowAccountsBackOrdering.Hint"] = "Allow backordering for this account",

                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.AllowAccountsAddressEditOnCheckout"] = "Allow Address Edit On Checkout",
                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.AllowAccountsAddressEditOnCheckout.Hint"] = "Allow editing address during checkout",

                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.LoyaltyBalance"] = "Loyalty Balance",
                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.LoyaltyBalance.Hint"] = "Loyalty points balance",

                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.MinimumOrderValue"] = "Minimum Order Value",
                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.MinimumOrderValue.Hint"] = "Minimum required order value",

                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.LoyaltyCardNumber"] = "Loyalty Card Number",
                ["Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.LoyaltyCardNumber.Hint"] = "Customer’s loyalty card number",

                #endregion

                #region product mapping

                ["Plugins.NopStation.D365Integration.MappingProduct.SystemName"] = "Mapping Product Data",
                ["Plugins.NopStation.D365Integration.MappingProduct.Title"] = "Product Settings",
                ["Plugins.NopStation.D365Integration.MappingProduct.Settings"] = "Product Data Settings",
                ["Plugins.NopStation.D365Integration.ErpProductDataSettings.SavedSuccessfully"] = "Product settings have been saved      successfully.",

                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.Name"] = "Name",
                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.Name.Hint"] = "The name of the product",

                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.ProductAttributes"] = "Product Attributes",
                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.ProductAttributes.Hint"] = "The attributes of the product",

                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.Sku"] = "SKU",
                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.Sku.Hint"] = "Stock Keeping Unit",

                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.ManufacturerPartNumber"] = "Manufacturer Part Number",
                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.ManufacturerPartNumber.Hint"] = "Part number assigned by the manufacturer",

                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.ShortDescription"] = "Short Description",
                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.ShortDescription.Hint"] = "A short summary of the product",

                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.FullDescription"] = "Full Description",
                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.FullDescription.Hint"] = "A detailed description of the product",

                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.Height"] = "Height",
                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.Height.Hint"] = "Height of the product",

                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.Width"] = "Width",
                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.Width.Hint"] = "Width of the product",

                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.Length"] = "Length",
                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.Length.Hint"] = "Length of the product",

                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.Weight"] = "Weight",
                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.Weight.Hint"] = "Weight of the product",

                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.Price"] = "Price",
                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.Price.Hint"] = "Selling price of the product",

                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.StockQuantity"] = "Stock Quantity",
                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.StockQuantity.Hint"] = "Available quantity in stock",

                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.ProductCategories"] = "Product Categories",
                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.ProductCategories.Hint"] = "Associated categories for the product",

                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.ProductAttributes"] = "Product Attributes",
                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.ProductAttributes.Hint"] = "Custom attributes for the product",

                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.TaxCategoryId"] = "Tax Category",
                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.TaxCategoryId.Hint"] = "ID of the associated tax category",

                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.TaxCategoryName"] = "Tax Category Name",
                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.TaxCategoryName.Hint"] = "Name of the tax category",

                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.Published"] = "Published",
                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.Published.Hint"] = "Indicates whether the product is published",

                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.ManufacturerName"] = "Manufacturer Name",
                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.ManufacturerName.Hint"] = "Name of the manufacturer",

                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.ManufacturerCode"] = "Manufacturer Code",
                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.ManufacturerCode.Hint"] = "Internal code for the manufacturer",

                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.VendorCode"] = "Vendor Code",
                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.VendorCode.Hint"] = "Internal code for the vendor",

                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.VendorName"] = "Vendor Name",
                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.VendorName.Hint"] = "Name of the vendor",

                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.ProductTags"] = "Product Tags",
                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.ProductTags.Hint"] = "Comma-separated list of product tags",

                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.LastChangedDate"] = "Last Changed Date",
                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.LastChangedDate.Hint"] = "The date the product was last modified",

                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.WarehouseNameOrCode"] = "Warehouse",
                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.WarehouseNameOrCode.Hint"] = "Name or code of the warehouse",

                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.Gtin"] = "GTIN",
                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.Gtin.Hint"] = "Global Trade Item Number",

                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.ProductCost"] = "Product Cost",
                ["Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.ProductCost.Hint"] = "Cost price of the product",

                ["Plugins.NopStation.D365Integration.MappingCategory.Title"] = "Mapping Category Settings",
                ["Plugins.NopStation.D365Integration.MappingCategory.Settings"] = "Category Data Settings",

                ["Plugins.NopStation.D365Integration.ErpCategoryDataSettingsModel.Fields.CategoryName"] = "Category Name",
                ["Plugins.NopStation.D365Integration.ErpCategoryDataSettingsModel.Fields.CategoryName.Hint"] = "Path to category name in ERP data",

                ["Plugins.NopStation.D365Integration.ErpCategoryDataSettingsModel.Fields.Description"] = "Description",
                ["Plugins.NopStation.D365Integration.ErpCategoryDataSettingsModel.Fields.Description.Hint"] = "Path to description in ERP data",

            #endregion

                #region ship to address mapping

                ["Plugins.NopStation.D365Integration.MappingShipToAddress.SystemName"] = "Mapping Ship To Address",
                ["Plugins.NopStation.D365Integration.MappingShipToAddress.Title"] = "Ship To Address Settings",
                ["Plugins.NopStation.D365Integration.MappingShipToAddress.Settings"] = "Ship To Address Settings",
                ["Plugins.NopStation.D365Integration.ErpShipToAddressSettings.SavedSuccessfully"] = "Ship To Address settings have been saved successfully.",

                ["Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.AccountNumber"] = "Account Number",
                ["Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.AccountNumber.Hint"] = "Account number",

                ["Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.ProvinceCode"] = "Province Code",
                ["Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.ProvinceCode.Hint"] = "Province/Region code",

                ["Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.ShipToCode"] = "Ship To Code",
                ["Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.ShipToCode.Hint"] = "Ship-to location code",

                ["Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.ShipToName"] = "Ship To Name",
                ["Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.ShipToName.Hint"] = "Name of the ship-to contact",

                ["Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.Company"] = "Company",
                ["Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.Company.Hint"] = "Company name",

                ["Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.Address1"] = "Address 1",
                ["Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.Address1.Hint"] = "Primary street address",

                ["Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.Address2"] = "Address 2",
                ["Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.Address2.Hint"] = "Secondary address (optional)",

                ["Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.City"] = "City",
                ["Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.City.Hint"] = "City or town name",

                ["Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.StateProvince"] = "State / Province",
                ["Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.StateProvince.Hint"] = "State or province name",

                ["Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.Suburb"] = "Suburb",
                ["Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.Suburb.Hint"] = "Suburb or locality",

                ["Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.County"] = "County",
                ["Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.County.Hint"] = "County (if applicable)",

                ["Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.Country"] = "Country",
                ["Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.Country.Hint"] = "Country name",

                ["Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.ZipPostalCode"] = "Zip / Postal Code",
                ["Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.ZipPostalCode.Hint"] = "Zip or postal code",

                ["Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.PhoneNumber"] = "Phone Number",
                ["Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.PhoneNumber.Hint"] = "Phone number of contact",

                ["Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.FaxNumber"] = "Fax Number",
                ["Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.FaxNumber.Hint"] = "Fax number (if available)",

                ["Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.CustomAttributes"] = "Custom Attributes",
                ["Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.CustomAttributes.Hint"] = "Any additional custom fields",

                ["Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.DeliveryNotes"] = "Delivery Notes",
                ["Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.DeliveryNotes.Hint"] = "Special delivery instructions",

                ["Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.EmailAddress"] = "Email Address",
                ["Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.EmailAddress.Hint"] = "Email for contact or confirmation",

                ["Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.RepNumber"] = "Representative Number",
                ["Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.RepNumber.Hint"] = "Sales representative ID",

                ["Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.RepFullName"] = "Representative Full Name",
                ["Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.RepFullName.Hint"] = "Full name of sales representative",

                ["Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.RepPhoneNumber"] = "Representative Phone Number",
                ["Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.RepPhoneNumber.Hint"] = "Contact number for sales rep",

                ["Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.RepEmail"] = "Representative Email",
                ["Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.RepEmail.Hint"] = "Email of sales rep",

                ["Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.SalesOrgCode"] = "Sales Org Code",
                ["Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.SalesOrgCode.Hint"] = "Sales organization identifier",

                #endregion

                #region stock
                ["Plugins.NopStation.D365Integration.MappingStock.SystemName"] = "Mapping Stock Data",
                ["Plugins.NopStation.D365Integration.MappingStock.Title"] = "Stock Settings",
                ["Plugins.NopStation.D365Integration.MappingStock.Settings"] = "Stock Data Settings",
                ["Plugins.NopStation.D365Integration.ErpStockSettings.SavedSuccessfully"] = "Stock data settings have been saved successfully.",
                ["Plugins.NopStation.D365Integration.ErpStockSettingModel.Fields.WarehouseNameOrCode"] = "Warehouse Code/Name",
                ["Plugins.NopStation.D365Integration.ErpStockSettingModel.Fields.WarehouseNameOrCode.Hint"] = "Warehouse code or name path",

                ["Plugins.NopStation.D365Integration.ErpStockSettingModel.Fields.Sku"] = "SKU",
                ["Plugins.NopStation.D365Integration.ErpStockSettingModel.Fields.Sku.Hint"] = "Product SKU path",

                ["Plugins.NopStation.D365Integration.ErpStockSettingModel.Fields.SalesOrgCode"] = "Sales Org Code",
                ["Plugins.NopStation.D365Integration.ErpStockSettingModel.Fields.SalesOrgCode.Hint"] = "Sales organization code path",

                ["Plugins.NopStation.D365Integration.ErpStockSettingModel.Fields.QuantityOnHand"] = "Quantity On Hand",
                ["Plugins.NopStation.D365Integration.ErpStockSettingModel.Fields.QuantityOnHand.Hint"] = "Quantity on hand path",

                ["Plugins.NopStation.D365Integration.ErpStockSettingModel.Fields.QuantityOnSalesOrder"] = "Quantity On Sales Order",
                ["Plugins.NopStation.D365Integration.ErpStockSettingModel.Fields.QuantityOnSalesOrder.Hint"] = "Quantity on sales order path",

                ["Plugins.NopStation.D365Integration.ErpStockSettingModel.Fields.LastChangedDate"] = "Last Changed Date",
                ["Plugins.NopStation.D365Integration.ErpStockSettingModel.Fields.LastChangedDate.Hint"] = "Date last changed path",

                #endregion

                #region order

                ["Plugins.NopStation.D365Integration.MappingPlaceOrder.SavedSuccessfully"] = "Mapping place order saved successfully",
                ["Plugins.NopStation.D365Integration.MappingPlaceOrder.Settings"] = "Mapping Place Order Header",
                ["Plugins.NopStation.D365Integration.MappingPlaceOrderItem.Settings"] = "Mapping Place Order Item",

                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.AccountNumber"] = "Account Number",
                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.AccountNumber.Hint"] = "Enter the customer's account number as registered in the ERP system.",

                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.SalesOrg"] = "Sales Org",
                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.SalesOrg.Hint"] = "Specify the sales organization responsible for the order.",

                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.CustomOrderNumber"] = "Custom Order Number",
                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.CustomOrderNumber.Hint"] = "Provide a custom reference number for the order.",

                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.ErpOrderNumber"] = "Erp Order Number",
                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.ErpOrderNumber.Hint"] = "Enter the order number assigned by the ERP system.",

                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.RepCode"] = "Rep Code",
                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.RepCode.Hint"] = "Provide the representative's code responsible for the sale.",

                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.AddressCode"] = "Address Code",
                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.AddressCode.Hint"] = "Specify the address code linked to this order.",

                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.ShippingAddress"] = "Shipping Address",
                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.ShippingAddress.Hint"] = "Enter the address where the order will be shipped.",

                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.BillingAddress"] = "Billing Address",
                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.BillingAddress.Hint"] = "Enter the address where the invoice will be sent.",

                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.CustomerName"] = "Customer Name",
                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.CustomerName.Hint"] = "Provide the full name of the customer placing the order.",

                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.Notes"] = "Notes",
                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.Notes.Hint"] = "Add any additional notes or comments related to the order.",

                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.CustomerReference"] = "Customer Reference",
                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.CustomerReference.Hint"] = "Provide a reference number from the customer, if available.",

                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.DeliveryInstruction"] = "Delivery Instruction",
                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.DeliveryInstruction.Hint"] = "Enter any special instructions for order delivery.",

                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.OrderCategory"] = "Order Category",
                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.OrderCategory.Hint"] = "Select the appropriate category for the order.",

                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.DeliveryMethod"] = "Delivery Method",
                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.DeliveryMethod.Hint"] = "Choose the method of delivery for this order.",

                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.CustomerFirstName"] = "Customer First Name",
                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.CustomerFirstName.Hint"] = "Enter the customer's first name.",

                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.CustomerLastName"] = "Customer Last Name",
                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.CustomerLastName.Hint"] = "Enter the customer's last name.",

                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.CustomerPhoneNumber"] = "Customer Phone Number",
                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.CustomerPhoneNumber.Hint"] = "Provide the customer's landline phone number.",

                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.CustomerMobileNumber"] = "Customer Mobile Number",
                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.CustomerMobileNumber.Hint"] = "Provide the customer's mobile phone number.",

                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.CustomerEmail"] = "Customer Email",
                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.CustomerEmail.Hint"] = "Enter the customer's email address.",

                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.VatNumber"] = "VAT Number",
                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.VatNumber.Hint"] = "Enter the customer's VAT (tax) identification number.",

                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.OrderType"] = "Order Type",
                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.OrderType.Hint"] = "Specify the type of order being placed (e.g., regular, express).",

                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.QuoteNumber"] = "Quote Number",
                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.QuoteNumber.Hint"] = "Provide the quote number associated with this order, if applicable.",

                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.OrderTax"] = "Order Tax",
                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.OrderTax.Hint"] = "Enter the total tax amount applied to the order.",

                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.OrderSubtotalExclTax"] = "Order Subtotal Excl. Tax",
                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.OrderSubtotalExclTax.Hint"] = "Enter the order's subtotal amount excluding tax.",

                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.OrderSubtotalInclTax"] = "Order Subtotal Incl. Tax",
                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.OrderSubtotalInclTax.Hint"] = "Enter the order's subtotal amount including tax.",

                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.CustomerCurrencyCode"] = "Customer Currency Code",
                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.CustomerCurrencyCode.Hint"] = "Specify the currency code used by the customer.",

                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.OrderDate"] = "Order Date",
                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.OrderDate.Hint"] = "Select the date when the order was placed.",

                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.DeliveryDate"] = "Delivery Date",
                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.DeliveryDate.Hint"] = "Enter the expected delivery date for the order.",

                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.DateRequired"] = "Date Required",
                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.DateRequired.Hint"] = "Specify the date by which the customer needs the order delivered.",

                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.ErpPlaceOrderItemDatas"] = "ERP Place Order Item Data",
                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.ErpPlaceOrderItemDatas.Hint"] = "Add the line item data for placing the ERP order.",

                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.AccountName"] = "Account Name",
                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.AccountName.Hint"] = "Enter the name associated with the account number.",

                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.ShippingAmount"] = "Shipping Amount",
                ["Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.ShippingAmount.Hint"] = "Specify the cost of shipping for this order.",


                #endregion

                #region Order Item

                ["Plugins.NopStation.D365Integration.MappingOrderItem.Title"] = "Mapping Order Item Settings",
                ["Plugins.NopStation.D365Integration.MappingOrderItem.Settings"] = "Order Item Data Settings",

                ["Plugins.NopStation.D365Integration.ErpOrderItemDataSettings.Fields.Sku"] = "SKU",
                ["Plugins.NopStation.D365Integration.ErpOrderItemDataSettings.Fields.Sku.Hint"] = "Enter the stock keeping unit (SKU) for the item.",

                ["Plugins.NopStation.D365Integration.ErpOrderItemDataSettings.Fields.BatchCode"] = "Batch Code",
                ["Plugins.NopStation.D365Integration.ErpOrderItemDataSettings.Fields.BatchCode.Hint"] = "Enter the batch code for inventory tracking.",

                ["Plugins.NopStation.D365Integration.ErpOrderItemDataSettings.Fields.Description"] = "Description",
                ["Plugins.NopStation.D365Integration.ErpOrderItemDataSettings.Fields.Description.Hint"] = "Provide a brief description of the item.",

                ["Plugins.NopStation.D365Integration.ErpOrderItemDataSettings.Fields.Quantity"] = "Quantity",
                ["Plugins.NopStation.D365Integration.ErpOrderItemDataSettings.Fields.Quantity.Hint"] = "Specify the quantity of the item being ordered.",

                ["Plugins.NopStation.D365Integration.ErpOrderItemDataSettings.Fields.UnitOfMeasure"] = "Unit of Measure",
                ["Plugins.NopStation.D365Integration.ErpOrderItemDataSettings.Fields.UnitOfMeasure.Hint"] = "Enter the unit of measurement (e.g., pcs, kg).",

                ["Plugins.NopStation.D365Integration.ErpOrderItemDataSettings.Fields.SpecialInstruction"] = "Special Instruction",
                ["Plugins.NopStation.D365Integration.ErpOrderItemDataSettings.Fields.SpecialInstruction.Hint"] = "Add any special instructions for this item.",

                ["Plugins.NopStation.D365Integration.ErpOrderItemDataSettings.Fields.UnitPriceExclTax"] = "Unit Price (Excl. Tax)",
                ["Plugins.NopStation.D365Integration.ErpOrderItemDataSettings.Fields.UnitPriceExclTax.Hint"] = "Enter the unit price excluding tax.",

                ["Plugins.NopStation.D365Integration.ErpOrderItemDataSettings.Fields.UnitPriceInclTax"] = "Unit Price (Incl. Tax)",
                ["Plugins.NopStation.D365Integration.ErpOrderItemDataSettings.Fields.UnitPriceInclTax.Hint"] = "Enter the unit price including tax.",

                ["Plugins.NopStation.D365Integration.ErpOrderItemDataSettings.Fields.DiscountPercentage"] = "Discount Percentage",
                ["Plugins.NopStation.D365Integration.ErpOrderItemDataSettings.Fields.DiscountPercentage.Hint"] = "Enter the discount percentage applied to the item.",

                ["Plugins.NopStation.D365Integration.ErpOrderItemDataSettings.Fields.DiscountAmountInclTax"] = "Discount Amount (Incl. Tax)",
                ["Plugins.NopStation.D365Integration.ErpOrderItemDataSettings.Fields.DiscountAmountInclTax.Hint"] = "Enter the total discount amount including tax.",

                ["Plugins.NopStation.D365Integration.ErpOrderItemDataSettings.Fields.DiscountAmountExclTax"] = "Discount Amount (Excl. Tax)",
                ["Plugins.NopStation.D365Integration.ErpOrderItemDataSettings.Fields.DiscountAmountExclTax.Hint"] = "Enter the total discount amount excluding tax.",

                ["Plugins.NopStation.D365Integration.ErpOrderItemDataSettings.Fields.PriceExclTax"] = "Price (Excl. Tax)",
                ["Plugins.NopStation.D365Integration.ErpOrderItemDataSettings.Fields.PriceExclTax.Hint"] = "Total price for the item excluding tax.",

                ["Plugins.NopStation.D365Integration.ErpOrderItemDataSettings.Fields.PriceInclTax"] = "Price (Incl. Tax)",
                ["Plugins.NopStation.D365Integration.ErpOrderItemDataSettings.Fields.PriceInclTax.Hint"] = "Total price for the item including tax.",

                #endregion

                #region shipping address

                ["Plugins.NopStation.D365Integration.MappingShippingAddress.Title"] = "Mapping Shipping Address Settings",
                ["Plugins.NopStation.D365Integration.MappingShippingAddress.Settings"] = "Shipping Address Settings",

                ["Plugins.NopStation.D365Integration.ErpAddressModel.Fields.Name"] = "Name",
                ["Plugins.NopStation.D365Integration.ErpAddressModel.Fields.Name.Hint"] = "Recipient's full name",

                ["Plugins.NopStation.D365Integration.ErpAddressModel.Fields.Email"] = "Email",
                ["Plugins.NopStation.D365Integration.ErpAddressModel.Fields.Email.Hint"] = "Email address of the recipient",

                ["Plugins.NopStation.D365Integration.ErpAddressModel.Fields.Company"] = "Company",
                ["Plugins.NopStation.D365Integration.ErpAddressModel.Fields.Company.Hint"] = "Company name (if applicable)",

                ["Plugins.NopStation.D365Integration.ErpAddressModel.Fields.Address1"] = "Address Line 1",
                ["Plugins.NopStation.D365Integration.ErpAddressModel.Fields.Address1.Hint"] = "Primary address line",

                ["Plugins.NopStation.D365Integration.ErpAddressModel.Fields.Address2"] = "Address Line 2",
                ["Plugins.NopStation.D365Integration.ErpAddressModel.Fields.Address2.Hint"] = "Additional address information (optional)",

                ["Plugins.NopStation.D365Integration.ErpAddressModel.Fields.Address3"] = "Address Line 3",
                ["Plugins.NopStation.D365Integration.ErpAddressModel.Fields.Address3.Hint"] = "Additional address information (optional)",

                ["Plugins.NopStation.D365Integration.ErpAddressModel.Fields.City"] = "City",
                ["Plugins.NopStation.D365Integration.ErpAddressModel.Fields.City.Hint"] = "City or locality",

                ["Plugins.NopStation.D365Integration.ErpAddressModel.Fields.StateProvince"] = "State/Province",
                ["Plugins.NopStation.D365Integration.ErpAddressModel.Fields.StateProvince.Hint"] = "State or province name",

                ["Plugins.NopStation.D365Integration.ErpAddressModel.Fields.Region"] = "Region",
                ["Plugins.NopStation.D365Integration.ErpAddressModel.Fields.Region.Hint"] = "Region or district name",

                ["Plugins.NopStation.D365Integration.ErpAddressModel.Fields.ZipPostalCode"] = "Zip/Postal Code",
                ["Plugins.NopStation.D365Integration.ErpAddressModel.Fields.ZipPostalCode.Hint"] = "Zip or postal code",

                ["Plugins.NopStation.D365Integration.ErpAddressModel.Fields.Country"] = "Country",
                ["Plugins.NopStation.D365Integration.ErpAddressModel.Fields.Country.Hint"] = "Country name",

                ["Plugins.NopStation.D365Integration.ErpAddressModel.Fields.PhoneNumber"] = "Phone Number",
                ["Plugins.NopStation.D365Integration.ErpAddressModel.Fields.PhoneNumber.Hint"] = "Contact phone number",

                ["Plugins.NopStation.D365Integration.ErpAddressModel.Fields.Suburb"] = "Suburb",
                ["Plugins.NopStation.D365Integration.ErpAddressModel.Fields.Suburb.Hint"] = "Suburb or neighborhood",

                #endregion

                #region billing address
                ["Plugins.NopStation.D365Integration.MappingBillingAddress.Title"] = "Mapping Billing  Address Settings",
                ["Plugins.NopStation.D365Integration.MappingBillingAddress.Settings"] = "Billing  Address Settings",
                #endregion

                #region invoice
                ["Plugins.NopStation.D365Integration.MappingInvoice.SystemName"] = "Mapping Invoice",
                ["Plugins.NopStation.D365Integration.MappingInvoice.Title"] = "Invoice Settings",
                ["Plugins.NopStation.D365Integration.MappingInvoice.Settings"] = "Invoice Settings",
                ["Plugins.NopStation.D365Integration.ErpInvoiceSettings.SavedSuccessfully"] = "Invoice settings have been saved successfully.",


                ["Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.PostingDateUtc"] = "Posting Date",
                ["Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.PostingDateUtc.Hint"] = "Path to the invoice posting date",

                ["Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.ErpDocumentNumber"] = "ERP Document Number",
                ["Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.ErpDocumentNumber.Hint"] = "Path to the ERP document number",

                ["Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.Description"] = "Description",
                ["Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.Description.Hint"] = "Path to the document description",

                ["Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.AmountInclVat"] = "Amount Including VAT",
                ["Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.AmountInclVat.Hint"] = "Path to amount including VAT",

                ["Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.AmountExclVat"] = "Amount Excluding VAT",
                ["Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.AmountExclVat.Hint"] = "Path to amount excluding VAT",

                ["Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.ErpAccountId"] = "ERP Account ID",
                ["Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.ErpAccountId.Hint"] = "Path to ERP account ID",

                ["Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.CurrencyCode"] = "Currency Code",
                ["Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.CurrencyCode.Hint"] = "Path to the currency code",

                ["Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.DocumentType"] = "Document Type",
                ["Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.DocumentType.Hint"] = "Path to the document type",

                ["Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.DocumentDisplayName"] = "Document Display Name",
                ["Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.DocumentDisplayName.Hint"] = "Path to the document display name",

                ["Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.PODSignedById"] = "POD Signed By ID",
                ["Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.PODSignedById.Hint"] = "Path to POD signer ID",

                ["Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.PODSignedOnUtc"] = "POD Signed On",
                ["Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.PODSignedOnUtc.Hint"] = "Path to POD signed date",

                ["Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.DueDateUtc"] = "Due Date",
                ["Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.DueDateUtc.Hint"] = "Path to invoice due date",

                ["Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.RelatedDocumentNo"] = "Related Document Number",
                ["Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.RelatedDocumentNo.Hint"] = "Path to the related document number",

                ["Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.ShipmentDateUtc"] = "Shipment Date",
                ["Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.ShipmentDateUtc.Hint"] = "Path to the shipment date",

                ["Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.DocumentDateUtc"] = "Document Date",
                ["Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.DocumentDateUtc.Hint"] = "Path to the document date",

                ["Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.ErpOrderNumber"] = "ERP Order Number",
                ["Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.ErpOrderNumber.Hint"] = "Path to the ERP order number",

                ["Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.Base64PDFData"] = "PDF Data (Base64)",
                ["Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.Base64PDFData.Hint"] = "Path to the base64 encoded PDF data",

                #endregion

                #region invoice pdf
                ["Plugins.NopStation.D365Integration.MappingInvoicePdf.SystemName"] = "Mapping Invoice Pdf",
                ["Plugins.NopStation.D365Integration.MappingInvoicePdf.Title"] = "Invoice Pdf Settings",
                ["Plugins.NopStation.D365Integration.MappingInvoicePdf.Settings"] = "Invoice Pdf Settings",
                ["Plugins.NopStation.D365Integration.ErpInvoicePdfSettings.SavedSuccessfully"] = "Invoice Pdf settings have been saved successfully.",

                ["Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.InvoicePDFData"] = "Invoice PDF feild mapping",
                ["Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.InvoicePDFData.Hint"] = "Path to the invoice PDF",
                #endregion

                #region order item additional data
                ["Plugins.NopStation.D365Integration.MappingOrderItemAdditionalData.Title"] = "Mapping Order Item Settings",
                ["Plugins.NopStation.D365Integration.MappingOrderItemAdditionalData.Settings"] = "Order Item Settings",

                ["Plugins.NopStation.D365Integration.ErpOrderItemAdditionalData.Fields.NopOrderItemId"] = "Nop Order Item ID",
                ["Plugins.NopStation.D365Integration.ErpOrderItemAdditionalData.Fields.NopOrderItemId.Hint"] = "Path to the NopCommerce order item ID",

                ["Plugins.NopStation.D365Integration.ErpOrderItemAdditionalData.Fields.ErpOrderId"] = "ERP Order ID",
                ["Plugins.NopStation.D365Integration.ErpOrderItemAdditionalData.Fields.ErpOrderId.Hint"] = "Path to the ERP order ID",

                ["Plugins.NopStation.D365Integration.ErpOrderItemAdditionalData.Fields.ErpOrderLineNumber"] = "ERP Order Line Number",
                ["Plugins.NopStation.D365Integration.ErpOrderItemAdditionalData.Fields.ErpOrderLineNumber.Hint"] = "Path to the ERP order line number",

                ["Plugins.NopStation.D365Integration.ErpOrderItemAdditionalData.Fields.ErpSalesUoM"] = "ERP Sales Unit of Measure",
                ["Plugins.NopStation.D365Integration.ErpOrderItemAdditionalData.Fields.ErpSalesUoM.Hint"] = "Path to the sales unit of measure from ERP",

                ["Plugins.NopStation.D365Integration.ErpOrderItemAdditionalData.Fields.ErpOrderLineStatus"] = "ERP Order Line Status",
                ["Plugins.NopStation.D365Integration.ErpOrderItemAdditionalData.Fields.ErpOrderLineStatus.Hint"] = "Path to the status of the ERP order line",

                ["Plugins.NopStation.D365Integration.ErpOrderItemAdditionalData.Fields.ErpDateRequired"] = "ERP Date Required",
                ["Plugins.NopStation.D365Integration.ErpOrderItemAdditionalData.Fields.ErpDateRequired.Hint"] = "Path to the required delivery date",

                ["Plugins.NopStation.D365Integration.ErpOrderItemAdditionalData.Fields.ErpDateExpected"] = "ERP Date Expected",
                ["Plugins.NopStation.D365Integration.ErpOrderItemAdditionalData.Fields.ErpDateExpected.Hint"] = "Path to the expected delivery date",

                ["Plugins.NopStation.D365Integration.ErpOrderItemAdditionalData.Fields.ErpDeliveryMethod"] = "ERP Delivery Method",
                ["Plugins.NopStation.D365Integration.ErpOrderItemAdditionalData.Fields.ErpDeliveryMethod.Hint"] = "Path to the delivery method specified in ERP",

                ["Plugins.NopStation.D365Integration.ErpOrderItemAdditionalData.Fields.ErpInvoiceNumber"] = "ERP Invoice Number",
                ["Plugins.NopStation.D365Integration.ErpOrderItemAdditionalData.Fields.ErpInvoiceNumber.Hint"] = "Path to the invoice number generated in ERP",

                ["Plugins.NopStation.D365Integration.ErpOrderItemAdditionalData.Fields.ErpOrderLineNotes"] = "ERP Order Line Notes",
                ["Plugins.NopStation.D365Integration.ErpOrderItemAdditionalData.Fields.ErpOrderLineNotes.Hint"] = "Path to the notes or remarks for the order line",

                ["Plugins.NopStation.D365Integration.ErpOrderItemAdditionalData.Fields.LastErpUpdateUtc"] = "Last ERP Update (UTC)",
                ["Plugins.NopStation.D365Integration.ErpOrderItemAdditionalData.Fields.LastErpUpdateUtc.Hint"] = "Path to the timestamp of the last ERP update in UTC",

                ["Plugins.NopStation.D365Integration.ErpOrderItemAdditionalData.Fields.ChangedOnUtc"] = "Changed On (UTC)",
                ["Plugins.NopStation.D365Integration.ErpOrderItemAdditionalData.Fields.ChangedOnUtc.Hint"] = "Path to the UTC date and time when the order line was last changed",

                ["Plugins.NopStation.D365Integration.ErpOrderItemAdditionalData.Fields.ChangedBy"] = "Changed By",
                ["Plugins.NopStation.D365Integration.ErpOrderItemAdditionalData.Fields.ChangedBy.Hint"] = "Path to the ID of the user who made the change",

                ["Plugins.NopStation.D365Integration.ErpOrderItemAdditionalData.Fields.WareHouse"] = "Warehouse",
                ["Plugins.NopStation.D365Integration.ErpOrderItemAdditionalData.Fields.WareHouse.Hint"] = "Path to the warehouse or storage location code",

                #endregion

                #region Special Price
                ["Plugins.NopStation.D365Integration.MappingSpecialPrice.SystemName"] = "Mapping Special Price Data",
                ["Plugins.NopStation.D365Integration.ErpPriceSpecialPricingDataSettings.SavedSuccessfully"] = "Price data settings have been saved successfully.",

                ["Plugins.NopStation.D365Integration.MappingSpecialPricing.Title"] = "Mapping Special Pricing",
                ["Plugins.NopStation.D365Integration.MappingSpecialPricing.Settings"] = "Mapping Special Pricing Settings",

                ["Plugins.NopStation.D365Integration.ErpPriceSpecialPricingDataSettingsModel.Fields.AccountNumber"] = "Account Number",
                ["Plugins.NopStation.D365Integration.ErpPriceSpecialPricingDataSettingsModel.Fields.AccountNumber.Hint"] = "ERP path for Account Number",

                ["Plugins.NopStation.D365Integration.ErpPriceSpecialPricingDataSettingsModel.Fields.Branch"] = "Branch",
                ["Plugins.NopStation.D365Integration.ErpPriceSpecialPricingDataSettingsModel.Fields.Branch.Hint"] = "ERP path for Branch",

                ["Plugins.NopStation.D365Integration.ErpPriceSpecialPricingDataSettingsModel.Fields.Sku"] = "SKU",
                ["Plugins.NopStation.D365Integration.ErpPriceSpecialPricingDataSettingsModel.Fields.Sku.Hint"] = "ERP path for SKU",

                ["Plugins.NopStation.D365Integration.ErpPriceSpecialPricingDataSettingsModel.Fields.SpecialPrice"] = "Special Price",
                ["Plugins.NopStation.D365Integration.ErpPriceSpecialPricingDataSettingsModel.Fields.SpecialPrice.Hint"] = "ERP path for Special Price",

                ["Plugins.NopStation.D365Integration.ErpPriceSpecialPricingDataSettingsModel.Fields.SellingPrice"] = "Selling Price",
                ["Plugins.NopStation.D365Integration.ErpPriceSpecialPricingDataSettingsModel.Fields.SellingPrice.Hint"] = "ERP path for Selling Price",

                ["Plugins.NopStation.D365Integration.ErpPriceSpecialPricingDataSettingsModel.Fields.PromoPrice"] = "Promo Price",
                ["Plugins.NopStation.D365Integration.ErpPriceSpecialPricingDataSettingsModel.Fields.PromoPrice.Hint"] = "ERP path for Promo Price",

                ["Plugins.NopStation.D365Integration.ErpPriceSpecialPricingDataSettingsModel.Fields.ListPrice"] = "List Price",
                ["Plugins.NopStation.D365Integration.ErpPriceSpecialPricingDataSettingsModel.Fields.ListPrice.Hint"] = "ERP path for List Price",

                ["Plugins.NopStation.D365Integration.ErpPriceSpecialPricingDataSettingsModel.Fields.RetailPrice"] = "Retail Price",
                ["Plugins.NopStation.D365Integration.ErpPriceSpecialPricingDataSettingsModel.Fields.RetailPrice.Hint"] = "ERP path for Retail Price",

                ["Plugins.NopStation.D365Integration.ErpPriceSpecialPricingDataSettingsModel.Fields.DiscountPercentage"] = "Discount %",
                ["Plugins.NopStation.D365Integration.ErpPriceSpecialPricingDataSettingsModel.Fields.DiscountPercentage.Hint"] = "ERP path for Discount Percentage",

                ["Plugins.NopStation.D365Integration.ErpPriceSpecialPricingDataSettingsModel.Fields.PricingNotes"] = "Pricing Notes",
                ["Plugins.NopStation.D365Integration.ErpPriceSpecialPricingDataSettingsModel.Fields.PricingNotes.Hint"] = "ERP path for Pricing Notes",
                #endregion

                #region Group Price
                ["Plugins.NopStation.D365Integration.MappingPriceGroup.SystemName"] = "Mapping Group Price Data",
                ["Plugins.NopStation.D365Integration.MappingPriceGroup.Title"] = "Group Price Settings",
                ["Plugins.NopStation.D365Integration.MappingPriceGroup.Settings"] = "Group Price Data Settings",
                ["Plugins.NopStation.D365Integration.ErpPriceGroupPricingDataSettings.SavedSuccessfully"] = "Price group data settings have been saved      successfully.",

                ["Plugins.NopStation.D365Integration.ErpPriceGroupPricingDataSettingsModel.Fields.GroupPriceCode"] = "Group Price Code",
                ["Plugins.NopStation.D365Integration.ErpPriceGroupPricingDataSettingsModel.Fields.GroupPriceCode.Hint"] = "JSON path to the group price code    field",

                ["Plugins.NopStation.D365Integration.ErpPriceGroupPricingDataSettingsModel.Fields.Sku"] = "SKU",
                ["Plugins.NopStation.D365Integration.ErpPriceGroupPricingDataSettingsModel.Fields.Sku.Hint"] = "JSON path to the SKU field",

                ["Plugins.NopStation.D365Integration.ErpPriceGroupPricingDataSettingsModel.Fields.Price"] = "Price",
                ["Plugins.NopStation.D365Integration.ErpPriceGroupPricingDataSettingsModel.Fields.Price.Hint"] = "JSON path to the price field",

                ["Plugins.NopStation.D365Integration.ErpPriceGroupPricingDataSettingsModel.Fields.GroupPrices"] = "Group Prices",
                ["Plugins.NopStation.D365Integration.ErpPriceGroupPricingDataSettingsModel.Fields.GroupPrices.Hint"] = "JSON path to the group prices dictionary",

                #endregion

                #region CreateAccount

                ["Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.SystemName"] = "Create Account Mappings",
                ["Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Title"] = "Create Account Mappings",
                ["Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Settings"] = "Create Account Mappings",
                ["Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.SavedSuccessfully"] = "Create account mappings have been saved successfully.",
                ["Plugins.NopStation.D365Integration.DuplicateKey"] = "Mappings could not be saved because the following keys are duplicated: {0}",
                ["Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Fields.AccountName"] = "Account Name",
                ["Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Fields.AccountName.Hint"] = "Name of the account",

                ["Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Fields.AccountNumber"] = "Account Number",
                ["Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Fields.AccountNumber.Hint"] = "Account number",

                ["Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Fields.Email"] = "Email",
                ["Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Fields.Email.Hint"] = "Email address of the account",

                ["Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Fields.Address1"] = "Address Line 1",
                ["Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Fields.Address1.Hint"] = "Primary address line",

                ["Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Fields.Address2"] = "Address Line 2",
                ["Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Fields.Address2.Hint"] = "Secondary address line",

                ["Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Fields.Address3"] = "Address Line 3",
                ["Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Fields.Address3.Hint"] = "Additional address line",

                ["Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Fields.ZipPostalCode"] = "Zip/Postal Code",
                ["Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Fields.ZipPostalCode.Hint"] = "Zip or postal code",

                ["Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Fields.PostalCode"] = "Postal Code",
                ["Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Fields.PostalCode.Hint"] = "Alternative postal code field",

                ["Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Fields.PhoneNumber"] = "Phone Number",
                ["Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Fields.PhoneNumber.Hint"] = "Contact phone number",

                ["Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Fields.FaxNumber"] = "Fax Number",
                ["Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Fields.FaxNumber.Hint"] = "Fax number if applicable",

                ["Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Fields.ContactName"] = "Contact Name",
                ["Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Fields.ContactName.Hint"] = "Full name of the contact person",

                ["Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Fields.City"] = "City",
                ["Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Fields.City.Hint"] = "City of the address",

                ["Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Fields.County"] = "County",
                ["Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Fields.County.Hint"] = "County or district",

                ["Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Fields.Country"] = "Country",
                ["Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Fields.Country.Hint"] = "Country name",

                ["Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Fields.StateProvince"] = "State/Province",
                ["Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Fields.StateProvince.Hint"] = "State or province",

                ["Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Fields.VatNumber"] = "VAT Number",
                ["Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Fields.VatNumber.Hint"] = "VAT registration number",

                ["Plugins.NopStation.D365Integration.AdditionalHardcodedValueModel.Fields.AdditionalHardCodedValues"] = "Additional HardCoded Value",
                ["Plugins.NopStation.D365Integration.AdditionalHardcodedValueModel.Fields.AdditionalHardCodedValues.Hint"] = "Additional hardCoded values for creating payload. enter the values that does not comes from the model and specify the key, value, and type.",
                #endregion

                ["Plugins.NopStation.BcsIntegration.ErpGetRequestSettingsModel.Fields.InvoiceSyncLimit"] = "Invoice Sync Limit",
                ["Plugins.NopStation.BcsIntegration.ErpGetRequestSettingsModel.Fields.InvoiceSyncLimit.Hint"] = "Maximum number of invoices to sync per request from ERP",

                ["Plugins.NopStation.BcsIntegration.ErpGetRequestSettingsModel.Fields.OrderSyncLimit"] = "Order Sync Limit",
                ["Plugins.NopStation.BcsIntegration.ErpGetRequestSettingsModel.Fields.OrderSyncLimit.Hint"] = "Maximum number of orders to sync per request from ERP",

                ["Plugins.NopStation.BcsIntegration.ErpGetRequestSettingsModel.Fields.StockSyncLimit"] = "Stock Sync Limit",
                ["Plugins.NopStation.BcsIntegration.ErpGetRequestSettingsModel.Fields.StockSyncLimit.Hint"] = "Maximum number of stock items to sync per request from ERP",

                ["Plugins.NopStation.BcsIntegration.ErpGetRequestSettingsModel.Fields.ShipToAddressSyncLimit"] = "Ship To Address Sync Limit",
                ["Plugins.NopStation.BcsIntegration.ErpGetRequestSettingsModel.Fields.ShipToAddressSyncLimit.Hint"] = "Maximum number of shipping addresses to sync per request from ERP",

                ["Plugins.NopStation.BcsIntegration.ErpGetRequestSettingsModel.Fields.AccountSyncLimit"] = "Account Sync Limit",
                ["Plugins.NopStation.BcsIntegration.ErpGetRequestSettingsModel.Fields.AccountSyncLimit.Hint"] = "Maximum number of customer accounts to sync per request from ERP",
                ["Plugins.NopStation.D365Integration.AdditionalFiltersModel.Fields.AdditionalFilters"] = "Additional Filters",
                ["Plugins.NopStation.D365Integration.AdditionalFiltersModel.Fields.AdditionalFilters.Hint"] = "Additional Filters",


                #region create shipToAddress

                ["Plugins.NopStation.D365Integration.ErpCreateShipToAddressSettingsModel.SystemName"] = "Create Ship-To Address Mappings",
                ["Plugins.NopStation.D365Integration.ErpCreateShipToAddressSettingsModel.Title"] = "Create Ship-To Address Mappings",
                ["Plugins.NopStation.D365Integration.MappingCreateShipToAddress.Settings"] = "Create Ship-To Address Mappings",
                ["Plugins.NopStation.D365Integration.ErpCreateShipToAddressSettingsModel.SavedSuccessfully"] = "Ship-to address mappings have been saved successfully.",
                ["Plugins.NopStation.D365Integration.ErpCreateShipToAddressSettingsModel.Fields.AccountNumber"] = "Account Number",
                ["Plugins.NopStation.D365Integration.ErpCreateShipToAddressSettingsModel.Fields.AccountNumber.Hint"] = "Enter the account number associated with this ship-to address.",
                ["Plugins.NopStation.D365Integration.ErpCreateShipToAddressSettingsModel.Fields.ShipToCode"] = "Ship-To Code",
                ["Plugins.NopStation.D365Integration.ErpCreateShipToAddressSettingsModel.Fields.ShipToCode.Hint"] = "Enter a unique code to identify this ship-to address.",
                ["Plugins.NopStation.D365Integration.ErpCreateShipToAddressSettingsModel.Fields.ShipToName"] = "Ship-To Name",
                ["Plugins.NopStation.D365Integration.ErpCreateShipToAddressSettingsModel.Fields.ShipToName.Hint"] = "Enter the name for this ship-to address.",
                ["Plugins.NopStation.D365Integration.ErpCreateShipToAddressSettingsModel.Fields.Country"] = "Country",
                ["Plugins.NopStation.D365Integration.ErpCreateShipToAddressSettingsModel.Fields.Country.Hint"] = "Select or enter the country for this ship-to address.",
                ["Plugins.NopStation.D365Integration.ErpCreateShipToAddressSettingsModel.Fields.City"] = "City",
                ["Plugins.NopStation.D365Integration.ErpCreateShipToAddressSettingsModel.Fields.City.Hint"] = "Enter the city for this ship-to address.",
                ["Plugins.NopStation.D365Integration.ErpCreateShipToAddressSettingsModel.Fields.Address1"] = "Address 1",
                ["Plugins.NopStation.D365Integration.ErpCreateShipToAddressSettingsModel.Fields.Address1.Hint"] = "Enter the primary address line for shipping.",
                ["Plugins.NopStation.D365Integration.ErpCreateShipToAddressSettingsModel.Fields.Address2"] = "Address 2",
                ["Plugins.NopStation.D365Integration.ErpCreateShipToAddressSettingsModel.Fields.Address2.Hint"] = "Enter the secondary address line for shipping, if any.",
                ["Plugins.NopStation.D365Integration.ErpCreateShipToAddressSettingsModel.Fields.PostalCode"] = "Postal Code",
                ["Plugins.NopStation.D365Integration.ErpCreateShipToAddressSettingsModel.Fields.PostalCode.Hint"] = "Enter the postal or ZIP code for this ship-to address.",
                ["Plugins.NopStation.D365Integration.ErpCreateShipToAddressSettingsModel.Fields.ShipToPhone"] = "Ship-To Phone",
                ["Plugins.NopStation.D365Integration.ErpCreateShipToAddressSettingsModel.Fields.ShipToPhone.Hint"] = "Enter the phone number for this ship-to address.",
                ["Plugins.NopStation.D365Integration.ErpCreateShipToAddressSettingsModel.Fields.ShipToEmail"] = "Ship-To Email",
                ["Plugins.NopStation.D365Integration.ErpCreateShipToAddressSettingsModel.Fields.ShipToEmail.Hint"] = "Enter the email address associated with this ship-to location.",


                #endregion

                #region JWT Auth

                ["Integration.IntegrationSecretKey.Hint"] = "Enter any 16-character key (letters and digits only) to represent your token generation.",
                ["Integration.IntegrationSecretKey"] = "Integration Token",
                ["Integration.Response.TokenExpired"] = "Token Expired",
                ["Integration.Response.InvalidToken"] = "Invalid Token",
                ["Integration.Login.CustomerRole"] = "Please check if the user has the Administration role",
                ["Integration.Login.Permission.Denied"] = "User doesn't have the Administration role",
                ["Integration.Login.InvalidIntegrationSecretKey"] = "Integration SecretKey Is Not Set.",
                #endregion

            };
        }

        #endregion

        #region Methods

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/D365Integration/Configure";
        }

        public override async Task InstallAsync()
        {
            await _localizationService.AddOrUpdateLocaleResourceAsync(GetLocaleResources());

            #region Locales
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.AccessTokenUrl", "Access Token URL");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.ClientId", "Client ID");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.ClientSecret", "Client Secret");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.BaseApiUrl", "Base API URL");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.CompanyName", "Company Name");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.DefaultCustomerId", "Default Customer ID");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.ErpCallTimeOut", "API Call Timeout (seconds)");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.HttpCallMaxRetries", "Max API Call Retries");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.HttpCallRestTimeInMinutes", "Rest Time Between Retries (minutes)");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.CustomerSyncLimit", "Customer Sync Batch Size");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.ProductSyncLimit", "Product Sync Batch Size");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.ProductSyncLimit.Hint", "Specify the number of products to sync in each batch. Default is 100.");



            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.AccessTokenUrl", "Access Token URL");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.AccessTokenUrl.Hint", "Enter the OAuth2 access token URL for authentication with Dynamics 365.");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.ClientId", "Client ID");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.ClientId.Hint", "Enter the client ID (application ID) from your Azure AD application registration.");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.ClientSecret", "Client Secret");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.ClientSecret.Hint", "Enter the client secret from your Azure AD application registration.");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.BaseApiUrl", "Base API URL");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.BaseApiUrl.Hint", "Enter the base URL for Dynamics 365 Business Central API endpoints.");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.Scope", "Scope");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.Scope.Hint", "Enter the Scope Dynamics 365 Business Central API endpoints.");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.CompanyName", "Company Id");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.CompanyName.Hint", "Enter your Dynamics 365 Business Central company id.");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.DefaultCustomerId", "Default Customer ID");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.DefaultCustomerId.Hint", "Select the default customer account to use for system operations.");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.ErpCallTimeOut", "API Call Timeout (seconds)");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.ErpCallTimeOut.Hint", "Specify the timeout period in seconds for API calls to Dynamics 365.");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.HttpCallMaxRetries", "Max API Call Retries");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.HttpCallMaxRetries.Hint", "Specify the maximum number of retry attempts for failed API calls.");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.HttpCallRestTimeInMinutes", "Rest Time Between Retries (minutes)");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.HttpCallRestTimeInMinutes.Hint", "Specify the waiting time in minutes between retry attempts.");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.CustomerSyncLimit", "Customer Sync Limit");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.CustomerSyncLimit.Hint", "Specify the number of customers to sync in each batch. Default is 100.");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.ProductSyncLimit", "Product Sync Limit");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.ProductSyncLimit.Hint", "Specify the number of products to sync in each batch. Default is 100.");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.StockSyncLimit", "Stock Sync Limit");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.StockSyncLimit.Hint", "Specify the number of stock items to sync in each batch. Default is 100.");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.OrderSyncLimit", "Order Sync BLimit");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.OrderSyncLimit.Hint", "Specify the number of Order Sync in each batch. Default is 100.");
            #endregion

            await base.InstallAsync();
        }

        public override async Task UpdateAsync(string currentVersion, string targetVersion)
        {
            if (currentVersion == targetVersion) return;

            #region Locales
            await _localizationService.AddOrUpdateLocaleResourceAsync(GetLocaleResources());

            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.AccessTokenUrl", "Access Token URL");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.AccessTokenUrl.Hint", "Enter the OAuth2 access token URL for authentication with Dynamics 365.");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.ClientId", "Client ID");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.ClientId.Hint", "Enter the client ID (application ID) from your Azure AD application registration.");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.ClientSecret", "Client Secret");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.ClientSecret.Hint", "Enter the client secret from your Azure AD application registration.");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.BaseApiUrl", "Base API URL");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.BaseApiUrl.Hint", "Enter the base URL for Dynamics 365 Business Central API endpoints.");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.CompanyName", "Company Id");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.CompanyName.Hint", "Enter your Dynamics 365 Business Central company id.");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.DefaultCustomerId", "Default Customer ID");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.DefaultCustomerId.Hint", "Select the default customer account to use for system operations.");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.ErpCallTimeOut", "API Call Timeout (seconds)");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.ErpCallTimeOut.Hint", "Specify the timeout period in seconds for API calls to Dynamics 365.");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.HttpCallMaxRetries", "Max API Call Retries");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.HttpCallMaxRetries.Hint", "Specify the maximum number of retry attempts for failed API calls.");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.HttpCallRestTimeInMinutes", "Rest Time Between Retries (minutes)");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.HttpCallRestTimeInMinutes.Hint", "Specify the waiting time in minutes between retry attempts.");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.CustomerSyncLimit", "Customer Sync Limit");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.CustomerSyncLimit.Hint", "Specify the number of customers to sync in each batch. Default is 100.");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.ProductSyncLimit", "Product Sync Limit");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.ProductSyncLimit.Hint", "Specify the number of products to sync in each batch. Default is 100.");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.StockSyncLimit", "Stock Sync Limit");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.StockSyncLimit.Hint", "Specify the number of stock items to sync in each batch. Default is 100.");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.OrderSyncLimit", "Order Sync BLimit");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.OrderSyncLimit.Hint", "Specify the number of Order Sync in each batch. Default is 100.");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Admin.Configure.Title", "BCS Integration configuration page");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365Integration.Admin.Configure.Settings", "BCS Integration settings");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Enums.NopStation.Plugin.B2B.ERPIntegrationCore.Enums.ErpDocumentType.Invoice", "Invoice");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Enums.NopStation.Plugin.B2B.ERPIntegrationCore.Enums.ErpDocumentType.Payment", "Payment");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Enums.NopStation.Plugin.B2B.ERPIntegrationCore.Enums.ErpDocumentType.AccountingDoc", "Accounting Doc");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Enums.NopStation.Plugin.B2B.ERPIntegrationCore.Enums.ErpDocumentType.Document", "Refund");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Enums.NopStation.Plugin.B2B.ERPIntegrationCore.Enums.ErpDocumentType.CreditMemo", "Credit Memo");

            #endregion
            await _localizationService.AddOrUpdateLocaleResourceAsync(GetLocaleResources());

            await base.UpdateAsync(currentVersion, targetVersion);
        }

        public override async Task UninstallAsync()
        {
            // Settings
            await _settingService.DeleteSettingAsync<D365IntegrationSettings>();

            // Locales
            await _localizationService.DeleteLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.AccessTokenUrl");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.ClientId");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.ClientSecret");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.BaseApiUrl");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.CompanyName");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.DefaultCustomerId");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.ErpCallTimeOut");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.HttpCallMaxRetries");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.NopStation.D365Integration.Configuration.Fields.HttpCallRestTimeInMinutes");

            await _localizationService.DeleteLocaleResourcesAsync(GetLocaleResources().Keys.ToList());

            await base.UninstallAsync();
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            throw new NotImplementedException();
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
                ChildNodes = [
                    new SiteMapNode
                    {
                        SystemName = CHILD_NODE_CONFIG_SYSTEM_NAME,
                        Title = CHILD_NODE_CONFIG_TITLE,
                        ControllerName = CHILD_NODE_CONFIG_CONTROLLER_NAME,
                        ActionName = CHILD_NODE_CONFIG_ACTION_NAME,
                        IconClass = CHILD_NODE_CONFIG_ICON_CLASS,
                        Visible = CHILD_NODE_CONFIG_VISIBLE
                    },
                    new SiteMapNode
                    {
                        SystemName = "NopStation.Plugin.Misc.B2B.ODataIntegration.ConfigureApiUrls",
                        Title = await _localizationService.GetResourceAsync("Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettings.Title"),
                        ControllerName = "D365Integration",
                        ActionName = "ConfigureApiUrls",
                        IconClass = CHILD_NODE_CONFIG_ICON_CLASS,
                        Visible = true,
                    },
                    new SiteMapNode
                    {
                        SystemName = "NopStation.Plugin.Misc.B2B.ODataIntegration.MappingProduct",
                        Title = await _localizationService.GetResourceAsync("Plugins.NopStation.D365Integration.MappingProduct.Title"),
                        ControllerName = "D365Integration",
                        ActionName = "MappingProduct",
                        IconClass = CHILD_NODE_CONFIG_ICON_CLASS,
                        Visible = true,
                    },
                    new SiteMapNode
                    {
                        SystemName = CHILD_NODE_MAPPING_ACCOUNT_SYSTEM_NAME,
                        Title = await _localizationService.GetResourceAsync(CHILD_NODE_MAPPING_ACCOUNT_TITLE),
                        ControllerName = CHILD_NODE_MAPPING_ACCOUNT_CONTROLLER_NAME,
                        ActionName = CHILD_NODE_MAPPING_ACCOUNT_ACTION_NAME,
                        IconClass = CHILD_NODE_CONFIG_ICON_CLASS,
                        Visible = CHILD_NODE_MAPPING_ACCOUNT_VISIBLE
                    },
                    new SiteMapNode
                    {
                        SystemName = "NopStation.Plugin.Misc.B2B.ODataIntegration.MappingShipToAddress",
                        Title = await _localizationService.GetResourceAsync("Plugins.NopStation.D365Integration.MappingShipToAddress.Title"),
                        ControllerName = "D365Integration",
                        ActionName = "MappingShipToAddress",
                        IconClass = CHILD_NODE_CONFIG_ICON_CLASS,
                        Visible = true,
                    },
                    new SiteMapNode
                    {
                        SystemName = "NopStation.Plugin.Misc.B2B.ODataIntegration.MappingStock",
                        Title = await _localizationService.GetResourceAsync("Plugins.NopStation.D365Integration.MappingStock.Title"),
                        ControllerName = "D365Integration",
                        ActionName = "MappingStock",
                        IconClass = CHILD_NODE_CONFIG_ICON_CLASS,
                        Visible = true,
                    },
                    new SiteMapNode
                    {
                        SystemName = "NopStation.Plugin.Misc.B2B.ODataIntegration.MappingOrder",
                        Title = await _localizationService.GetResourceAsync("Plugins.NopStation.D365Integration.MappingOrder.Title"),
                        ControllerName = "D365Integration",
                        ActionName = "MappingOrder",
                        IconClass = CHILD_NODE_CONFIG_ICON_CLASS,
                        Visible = true,
                    },

                    new SiteMapNode
                    {
                        SystemName = "NopStation.Plugin.Misc.B2B.ODataIntegration.MappingInvoice",
                        Title = await _localizationService.GetResourceAsync("Plugins.NopStation.D365Integration.MappingInvoice.Title"),
                        ControllerName = "D365Integration",
                        ActionName = "MappingInvoice",
                        IconClass = CHILD_NODE_CONFIG_ICON_CLASS,
                        Visible = true,
                    },
                    new SiteMapNode
                    {
                        SystemName = "NopStation.Plugin.Misc.B2B.ODataIntegration.MappingInvoicePdf",
                        Title = await _localizationService.GetResourceAsync("Plugins.NopStation.D365Integration.MappingInvoicePdf.Title"),
                        ControllerName = "D365Integration",
                        ActionName = "MappingInvoicePdf",
                        IconClass = CHILD_NODE_CONFIG_ICON_CLASS,
                        Visible = true,
                    },
                    new SiteMapNode
                    {
                        SystemName = "NopStation.Plugin.Misc.B2B.ODataIntegration.MappingSpecialPrice",
                        Title = await _localizationService.GetResourceAsync("Plugins.NopStation.D365Integration.MappingSpecialPrice.Title"),
                        ControllerName = "D365Integration",
                        ActionName = "MappingSpecialPricing",
                        IconClass = CHILD_NODE_CONFIG_ICON_CLASS,
                        Visible = true,
                    },
                    new SiteMapNode
                    {
                        SystemName = "NopStation.Plugin.Misc.B2B.ODataIntegration.MappingGroupPricing",
                        Title = await _localizationService.GetResourceAsync("Plugins.NopStation.D365Integration.MappingGroupPricing.Title"),
                        ControllerName = "D365Integration",
                        ActionName = "MappingGroupPricing",
                        IconClass = CHILD_NODE_CONFIG_ICON_CLASS,
                        Visible = true,
                    },
                    new SiteMapNode
                    {
                        SystemName = "NopStation.Plugin.Misc.B2B.ODataIntegration.MappingPlaceOrder",
                        Title = await _localizationService.GetResourceAsync("Plugins.NopStation.D365Integration.MappingPlaceOrder.Title"),
                        ControllerName = "D365Integration",
                        ActionName = "MappingPlaceOrder",
                        IconClass = CHILD_NODE_CONFIG_ICON_CLASS,
                        Visible = true,
                    },
                    new SiteMapNode
                    {
                        SystemName = "NopStation.Plugin.Misc.B2B.ODataIntegration.CreateAccountMappings",
                        Title = await _localizationService.GetResourceAsync("Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Title"),
                        ControllerName = "D365Integration",
                        ActionName = "MappingCreateAccount",
                        IconClass = CHILD_NODE_CONFIG_ICON_CLASS,
                        Visible = true,
                    },
                    new SiteMapNode
                    {
                        SystemName = "NopStation.Plugin.Misc.B2B.ODataIntegration.CreateShipToAddressMappings",
                        Title = await _localizationService.GetResourceAsync("Plugins.NopStation.D365Integration.ErpCreateShipToAddressSettingsModel.Title"),
                        ControllerName = "D365Integration",
                        ActionName = "MappingCreateShipToAddress",
                        IconClass = CHILD_NODE_CONFIG_ICON_CLASS,
                        Visible = true,
                    },
                ], 
            });
        }

        public async Task<ErpResponseModel> CreateAccountNoErpAsync(ErpCreateAccountModel erpCreateAccountModel)
        {
            return await _d365Service.CreateAccountNoErpAsync(erpCreateAccountModel);
        }

        public async Task<ErpResponseData<ErpShipToAddressDataModel>> CreateShipToAddressOnErpAsync(ErpShipToAddressCreateModel erpShipToAddressCreateModel)
        {
            return await _d365Service.CreateShipToAddressOnErpAsync(erpShipToAddressCreateModel);
        }

        public async Task<ErpResponseData<ErpAccountDataModel>> GetAccountFromErpAsync(ErpGetRequestModel erpRequest)
        {
            //return await _d365IntegrationAccountService.GetAccountFromErpAsync(erpRequest);
            throw new NotImplementedException();
        }

        public async Task<ErpResponseData<IList<ErpAccountDataModel>>> GetAccountsFromErpAsync(ErpGetRequestModel erpRequest)
        {
            return await _d365Service.GetCustomersFromErpAsync(erpRequest);
        }

        public async Task<ErpResponseData<IList<ErpInvoiceDataModel>>> GetInvoiceByAccountNoFromErpAsync(ErpGetRequestModel erpRequest)
        {
            return await _d365Service.GetInvoiceByAccountNoFromErpAsync(erpRequest);
        }

        public async Task<ErpResponseData<string>> GetInvoicePdfByteCodeByDocumentNoFromErpAsync(ErpGetRequestModel erpRequest)
        {
            return await _d365Service.GetInvoicePdfByteCodeByDocumentNoFromErpAsync(erpRequest);
        }

        public Task<ErpResponseData<IList<ErpShipToAddressDataModel>>> GetShipToAddressByAccountNumberFromErpAsync(ErpGetRequestModel erpRequest)
        {
            throw new NotImplementedException();
        }

        public async Task<ErpResponseData<ErpProductDataModel>> GetProductByItemNoFromErpAsync(ErpGetRequestModel erpRequest)
        {
            throw new NotImplementedException();
        }

        public async Task<ErpResponseData<IList<ErpProductDataModel>>> GetProductsFromErpAsync(ErpGetRequestModel erpRequest)
        {
            return await _d365Service.GetProductsFromErpAsync(erpRequest);
        }

        public async Task<ErpResponseData<ErpStockDataModel>> GetStockByItemNoFromErpAsync(ErpGetRequestModel erpRequest)
        {
            throw new NotImplementedException();
        }

        public async Task<ErpResponseData<IList<ErpStockDataModel>>> GetStocksFromErpAsync(ErpGetRequestModel erpRequest)
        {
            return await _d365Service.GetStocksFromErpAsync(erpRequest);
        }
        public Task<ErpResponseData<ErpPriceGroupPricingDataModel>> GetProductGroupPriceFromErpAsync(ErpGetRequestModel erpRequest)
        {
            throw new NotImplementedException();
        }

        public async Task<ErpResponseData<IList<ErpPriceGroupPricingDataModel>>> GetProductGroupPricesFromErpAsync(ErpGetRequestModel erpRequest)
        {
            return await _d365Service.GetProductGroupPricingFromErpAsync( erpRequest);
        }

        public Task<ErpResponseData<ErpPriceSpecialPricingDataModel>> GetProductSpecialPriceFromErpAsync(ErpGetRequestModel erpRequest)
        {
            throw new NotImplementedException();
        }

        public async Task<ErpResponseData<IList<ErpPriceSpecialPricingDataModel>>> GetProductSpecialPricesFromErpAsync(ErpGetRequestModel erpRequest)
        {
            return await _d365Service.GetPerAccountProductPricingFromErpAsync(erpRequest);
        }

        public Task ProductListLiveStockDataAsync(ErpAccount erpAccount, IList<Product> products, IProductService productService)
        {
            throw new NotImplementedException();
        }

        public async Task<ErpResponseData<IList<ErpPlaceOrderDataModel>>> GetOrderByAccountFromErpAsync(ErpGetRequestModel erpRequest)
        {
            return await _d365Service.GetOrdersByAccountFromErpAsync(erpRequest);
        }

        public async Task<ErpResponseData<IList<ErpPlaceOrderDataModel>>> GetOrderByOrderNumberFromErpAsync(ErpGetRequestModel erpRequest)
        {
            throw new NotImplementedException();
        }

        public Task<ErpResponseData<IList<ErpPlaceOrderDataModel>>> GetQuoteByAccountFromErpAsync(ErpGetRequestModel erpRequest)
        {
            throw new NotImplementedException();
        }

        public Task<ErpResponseData<IList<ErpPlaceOrderDataModel>>> GetQuoteByQuoteNumberFromErpAsync(ErpGetRequestModel erpRequest)
        {
            throw new NotImplementedException();
        }

        public async Task<ErpResponseData<Dictionary<int, string>>> CreateOrderOnErpAsync(ErpPlaceOrderDataModel erpRequest, List<int> orderItemIds = null)
        {
            var result = await _d365Service.CreateOrderAsync(erpRequest, orderItemIds);
            return result;
        }

        public Task<ErpResponseModel> GetSalesOrgsFromErpAsync(ErpGetRequestModel erpRequest)
        {
            throw new NotImplementedException();
        }

        public Task<ErpResponseModel> GetSalesWarehouseFromErpAsync(ErpGetRequestModel erpRequest)
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetSalesOrgCodeFromIntegrationSettings()
        {
            return "001";
        }

        public Task<ErpResponseData<string>> GetStatementPdfByteCodeFromErpAsync(ErpGetRequestModel erpRequest)
        {
            throw new NotImplementedException();
        }

        public async Task<ErpResponseData<IList<ErpShipToAddressDataModel>>> GetShipToAddressesFromErpAsync(ErpGetRequestModel erpRequest)
        {
            return await _d365Service.GetShipToAddressFromErpAsync(erpRequest);
        }

        public Task<ErpResponseData<IList<ErpAccountDataModel>>> GetAllAccountCreditFromErpAsync(ErpGetRequestModel erpRequest)
        {
            throw new NotImplementedException();
        }

        public Task<ErpResponseData<IList<ErpPriceSpecialPricingDataModel>>> ProductListLivePriceSync(ErpGetRequestModel erpGetRequest)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}