using System.Dynamic;
using System.Reflection;
using System.Xml.Linq;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Services.Configuration;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.Misc.ErpSqlIntegration.Models;
using NopStation.Plugin.Misc.ErpSqlIntegration.Settings;

namespace NopStation.Plugin.Misc.ErpSqlIntegration.Services;

/// <summary>
/// Represents SqlIntegration manager
/// </summary>
public partial class SqlIntegrationService : ISqlIntegrationService
{
    private readonly SqlIntegrationSettings _sqlIntegrationSettings;
    private readonly IStoreContext _storeContext;
    private readonly ISettingService _settingService;
    private readonly IErpLogsService _erpLogsService;
    private readonly ErpPlaceOrderSettings _erpPlaceOrderSettings;
    private readonly ErpPlaceOrderItemSettings _erpPlaceOrderItemSettings;
    private readonly HashSet<string> _validTypes = new HashSet<string>
    {
        "string", "int", "bool", "decimal", "datetime"
    };

    public SqlIntegrationService(SqlIntegrationSettings sqlIntegrationSettings,
        IStoreContext storeContext,
        ISettingService settingService,
        IErpLogsService erpLogsService,
        ErpPlaceOrderSettings erpPlaceOrderSettings,
        ErpPlaceOrderItemSettings erpPlaceOrderItemSettings)
    {
        _sqlIntegrationSettings = sqlIntegrationSettings;
        _storeContext = storeContext;
        _settingService = settingService;
        _erpLogsService = erpLogsService;
        _erpPlaceOrderSettings = erpPlaceOrderSettings;
        _erpPlaceOrderItemSettings = erpPlaceOrderItemSettings;
    }

    public Task<bool> IsValidSqlIntegrationSettings()
    {
        if (string.IsNullOrEmpty(_sqlIntegrationSettings.ConnectionString))
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }

    public Task<(bool IsValid, string ErrorMessage)> IsValidSqlIntegrationSettingsForPlacingOrder()
    {
        if (string.IsNullOrEmpty(_sqlIntegrationSettings.BaseUrl))
        {
            return Task.FromResult((false, "Base URL is required."));
        }

        if (string.IsNullOrEmpty(_sqlIntegrationSettings.AuthPassword))
        {
            return Task.FromResult((false, "Auth password is required."));
        }

        if (string.IsNullOrEmpty(_sqlIntegrationSettings.AuthUserName))
        {
            return Task.FromResult((false, "Auth username is required."));
        }

        if (string.IsNullOrEmpty(_erpPlaceOrderItemSettings.ErpOrderPayloadLinesKey))
        {
            return Task.FromResult((false, "ErpOrderPayloadLinesKey is required."));
        }

        if (string.IsNullOrEmpty(_erpPlaceOrderSettings.ErpOrderPayloadRootKey))
        {
            return Task.FromResult((false, "ErpOrderPayloadRootKey is required."));
        }

        if (!Uri.IsWellFormedUriString(_sqlIntegrationSettings.BaseUrl, UriKind.Absolute))
        {
            return Task.FromResult((false, "Base URL is not a valid absolute URI."));
        }

        return Task.FromResult((true, string.Empty));
    }

    public object? ConvertValueWithType(string value, string type)
    {
        if (value == null || value.Trim().ToLowerInvariant() == "null")
            return null;

        return type?.ToLowerInvariant() switch
        {
            "int" => int.TryParse(value, out var i) ? i : null,
            "decimal" => decimal.TryParse(value, out var d) ? d : null,
            "bool" => bool.TryParse(value, out var b) ? b : null,
            "datetime" => DateTime.TryParse(value, out var dt) ? dt : null,
            "string" => value,
            _ => value // fallback
        };
    }

    public async Task<Dictionary<string, object>> ParseHardcodedValuesFromSettingAsync<TSettings>(string jsonSetting, TSettings settings)// where TSettings : ISettings
    {
        var dictionary = new Dictionary<string, object>();

        if (string.IsNullOrWhiteSpace(jsonSetting))
            return dictionary;

        try
        {
            var items = JsonConvert.DeserializeObject<List<AdditionalValues>>(jsonSetting);

            if (items == null)
                return dictionary;

            foreach (var item in items)
            {
                if (string.IsNullOrWhiteSpace(item?.Key))
                    continue;

                object value = ConvertValueWithType(item.Value, item.Type);
                dictionary[item.Key] = value;
            }
        }
        catch (JsonException ex)
        {
            await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Error, ErpSyncLevel.Order, $"Something went wrong while parsing hard coded values from settings. Error: {ex.Message}", ex.StackTrace);
        }

        return dictionary;
    }

    private bool IsSimpleType(Type type)
    {
        if (Nullable.GetUnderlyingType(type) is Type underlyingType)
            type = underlyingType;

        return
            type.IsPrimitive ||
            type.IsEnum ||
            type == typeof(string) ||
            type == typeof(DateTime) ||
            type == typeof(Guid);
    }

    private bool IsOtherType(Type type)
    {
        if (Nullable.GetUnderlyingType(type) is Type underlyingType)
            type = underlyingType;

        return
            type == typeof(bool) ||
            type == typeof(double) ||
            type == typeof(decimal) ||
            type == typeof(float) ||
            type == typeof(long) ||
            type == typeof(int) ||
            type == typeof(short);
    }

    private string ConvertToString(object value, Type type)
    {
        if (value == null)
            return null;

        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        if (underlyingType == typeof(DateTime))
            return ((DateTime)value).ToString("yyyy-MM-dd");

        if (underlyingType == typeof(bool))
            return ((bool)value) ? "true" : "false";

        return value.ToString();
    }

    private object ConvertToValue(object value, Type type)
    {
        if (value == null)
            return null;

        if (Nullable.GetUnderlyingType(type) is Type underlyingType)
            type = underlyingType;

        if (
            type == typeof(bool) ||
            type == typeof(double) ||
            type == typeof(float) ||
            type == typeof(decimal) ||
            type == typeof(long) ||
            type == typeof(int) ||
            type == typeof(short))
        {
            return value;
        }

        return null;
    }

    private async Task<IDictionary<string, object>> PrepareRequestBodyWithConfigurableParamsAsync<TRequest, TSettings>(TRequest erpRequest, TSettings settings) //where TSettings : AdditionalHardcodedValueSettings
    {
        var dict = new Dictionary<string, object>();

        if (erpRequest == null || settings == null)
            return dict;

        var requestProps = erpRequest.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var settingsProps = settings.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        string additionalHardCodedVlaues = "AdditionalHardCodedValues".ToLowerInvariant();

        foreach (var prop in settingsProps)
        {
            var settingValue = prop.GetValue(settings) as string;

            if (string.IsNullOrWhiteSpace(settingValue))
                continue;

            // for AdditionalHardCodedValues, parse the JSON and merge it into the dictionary
            if (prop.Name.ToLowerInvariant().Equals(additionalHardCodedVlaues))
            {
                var jsonParams = await ParseHardcodedValuesFromSettingAsync(settingValue, settings);
                foreach (var kvp in jsonParams)
                {
                    if (!dict.ContainsKey(kvp.Key))
                        dict[kvp.Key] = kvp.Value;
                }
                continue;
            }

            var matchingProp = requestProps.FirstOrDefault(p => p.Name == prop.Name);
            if (matchingProp == null)
                continue;

            var value = matchingProp.GetValue(erpRequest);
            if (value == null)
                continue;

            if (IsSimpleType(matchingProp.PropertyType))
            {
                dict[settingValue] = ConvertToString(value, matchingProp.PropertyType);
            }
            else if (IsOtherType((matchingProp.PropertyType)))
            {
                var val = ConvertToValue(value, matchingProp.PropertyType);
                if (val != null)
                {
                    dict[settingValue] = val;
                }
            }
        }

        return dict;
    }

    public async Task<dynamic> PrepareErpAccountsRequestBody(ErpGetRequestModel erpRequest)
    {
        if (erpRequest != null)
        {
            var start = int.Parse(erpRequest.Start);

            dynamic sqlRequest = new ExpandoObject();
            sqlRequest.action = "SQL";
            sqlRequest.name = "Online_GetCustomer";

            dynamic args = new ExpandoObject();
            args.Company = string.IsNullOrWhiteSpace(erpRequest.Location) ? null : erpRequest.Location;
            args.LastChangeDate = erpRequest.DateFrom.HasValue ? erpRequest.DateFrom.Value.ToString("yyyy-MM-ddTHH:mm:ss") : null;
            args.Customer = string.IsNullOrWhiteSpace(erpRequest.AccountNumber) ? null : erpRequest.AccountNumber;
            args.PageNumber = start > 0 ? start : 1;
            args.RowsPerPage = erpRequest.Limit > 0 ? erpRequest.Limit : 100;

            sqlRequest.args = args;
            erpRequest.Start = args.PageNumber.ToString();

            return sqlRequest;
        }

        return new ExpandoObject();
    }

    public async Task<dynamic> PrepareErpAccountCreditsRequestBody(ErpGetRequestModel erpRequest)
    {
        if (erpRequest != null)
        {
            var start = int.Parse(erpRequest.Start);

            dynamic sqlRequest = new ExpandoObject();
            sqlRequest.action = "SQL";
            sqlRequest.name = "Online_GetCustomerCreditInfo";

            dynamic args = new ExpandoObject();
            args.Company = string.IsNullOrWhiteSpace(erpRequest.Location) ? null : erpRequest.Location;
            args.LastChangeDate = erpRequest.DateFrom.HasValue ? erpRequest.DateFrom.Value.ToString("yyyy-MM-ddTHH:mm:ss") : null;
            args.Customer = string.IsNullOrWhiteSpace(erpRequest.AccountNumber) ? null : erpRequest.AccountNumber;
            args.PageNumber = start > 0 ? start : 1;
            args.RowsPerPage = erpRequest.Limit > 0 ? erpRequest.Limit : 100;

            sqlRequest.args = args;
            erpRequest.Start = args.PageNumber.ToString();

            return sqlRequest;
        }

        return new ExpandoObject();
    }

    public async Task<dynamic> PrepareErpPriceSpecialPricingsRequestBody(ErpGetRequestModel erpRequest)
    {
        if (erpRequest != null)
        {
            var start = int.Parse(erpRequest.Start);

            dynamic sqlRequest = new ExpandoObject();
            sqlRequest.action = "SQL";
            sqlRequest.name = "Online_GetPrice";

            dynamic args = new ExpandoObject();
            if (erpRequest.DateFrom < new DateTime(1753, 01, 01))
                args.LastChangeDate = null;
            else
                args.LastChangeDate = erpRequest.DateFrom.HasValue ? erpRequest.DateFrom.Value.ToString("yyyy-MM-ddTHH:mm:ss") : null;

            args.Company = string.IsNullOrWhiteSpace(erpRequest.Location) ? null : erpRequest.Location;
            args.Customer = string.IsNullOrWhiteSpace(erpRequest.AccountNumber) ? null : erpRequest.AccountNumber;
            args.StockCode = string.IsNullOrWhiteSpace(erpRequest.ProductSku) ? null : erpRequest.ProductSku;
            args.PageNumber = start > 0 ? start : 1;
            erpRequest.Start = args.PageNumber.ToString();
            args.RowsPerPage = erpRequest.Limit > 0 ? erpRequest.Limit : 100;

            sqlRequest.args = args;

            return sqlRequest;
        }

        return new ExpandoObject();
    }

    public async Task<dynamic> PrepareErpProductRequestBody(ErpGetRequestModel erpRequest)
    {
        if (erpRequest != null)
        {
            var start = int.Parse(erpRequest.Start);

            dynamic sqlRequest = new ExpandoObject();
            sqlRequest.action = "SQL";
            sqlRequest.name = "Online_GetStockData";

            dynamic args = new ExpandoObject();
            args.Company = string.IsNullOrWhiteSpace(erpRequest.Location) ? null : erpRequest.Location;
            args.LastChangeDate = erpRequest.DateFrom.HasValue ? erpRequest.DateFrom.Value.ToString("yyyy-MM-ddTHH:mm:ss") : null;
            args.StockCode = string.IsNullOrWhiteSpace(erpRequest.ProductSku) ? null : erpRequest.ProductSku;
            args.PageNumber = start > 0 ? start : 1;
            erpRequest.Start = args.PageNumber.ToString();
            args.RowsPerPage = erpRequest.Limit > 0 ? erpRequest.Limit : 100;

            sqlRequest.args = args;

            return sqlRequest;
        }

        return new ExpandoObject();
    }

    public async Task<dynamic> PrepareErpShipToAddressRequestBody(ErpGetRequestModel erpRequest)
    {
        if (erpRequest != null)
        {
            var start = int.Parse(erpRequest.Start);

            dynamic sqlRequest = new ExpandoObject();
            sqlRequest.action = "SQL";
            sqlRequest.name = "Online_GetCustomerAddr";

            dynamic args = new ExpandoObject();
            args.Company = string.IsNullOrWhiteSpace(erpRequest.Location) ? null : erpRequest.Location;
            args.Customer = string.IsNullOrWhiteSpace(erpRequest.AccountNumber) ? null : erpRequest.AccountNumber;
            args.PageNumber = start > 0 ? start : 1;
            erpRequest.Start = args.PageNumber.ToString();
            args.RowsPerPage = erpRequest.Limit > 0 ? erpRequest.Limit : 100;

            sqlRequest.args = args;

            return sqlRequest;
        }

        return new ExpandoObject();
    }

    public async Task<dynamic> PrepareErpStockRequestBody(ErpGetRequestModel erpRequest)
    {
        if (erpRequest != null)
        {
            var start = int.Parse(erpRequest.Start);

            dynamic sqlRequest = new ExpandoObject();
            sqlRequest.action = "SQL";
            sqlRequest.name = "Online_GetStockOnHand";

            dynamic args = new ExpandoObject();
            args.Company = string.IsNullOrWhiteSpace(erpRequest.Location) ? null : erpRequest.Location;
            args.LastChangeDate = erpRequest.DateFrom.HasValue ? erpRequest.DateFrom.Value.ToString("yyyy-MM-ddTHH:mm:ss") : null;
            args.StockCode = string.IsNullOrWhiteSpace(erpRequest.ProductSku) ? null : erpRequest.ProductSku;
            args.PageNumber = start > 0 ? start : 1;
            args.RowsPerPage = erpRequest.Limit > 0 ? erpRequest.Limit : 100;

            sqlRequest.args = args;
            erpRequest.Start = args.PageNumber.ToString();

            return sqlRequest;
        }

        return new ExpandoObject();
    }

    public async Task<dynamic> PrepareErpOrderPlaceRequestBodyAsync(ErpPlaceOrderDataModel erpRequest)
    {
        if (erpRequest is null)
            return null;

        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var erpPlaceOrderSettings = await _settingService.LoadSettingAsync<ErpPlaceOrderSettings>(storeScope);
        var erpPlaceOrderItemSettings = await _settingService.LoadSettingAsync<ErpPlaceOrderItemSettings>(storeScope);

        // Prepare order details
        var orderDetailsDict = await PrepareRequestBodyWithConfigurableParamsAsync(erpRequest, erpPlaceOrderSettings)
                                ?? new Dictionary<string, object>();

        dynamic root = new ExpandoObject();
        var rootDict = (IDictionary<string, object>)root;

        rootDict[erpPlaceOrderSettings.ErpOrderPayloadRootKey] = orderDetailsDict;

        // Prepare line items
        List<object> orderLines = new List<object>();
        foreach (var item in erpRequest.ErpPlaceOrderItemDatas)
        {
            var orderLineDict = await PrepareRequestBodyWithConfigurableParamsAsync(item, erpPlaceOrderItemSettings);
            if (orderLineDict != null)
                orderLines.Add(orderLineDict);
        }

        orderDetailsDict[erpPlaceOrderItemSettings.ErpOrderPayloadLinesKey] = orderLines;

        return root;
    }

    public async Task<dynamic> PrepareErpOrderPlaceRequestBody(ErpPlaceOrderDataModel erpRequest)
    {
        if (erpRequest != null)
        {
            var orderDetails = new List<dynamic>();
            foreach (var item in erpRequest.ErpPlaceOrderItemDatas)
            {
                dynamic stockLine = new ExpandoObject();
                stockLine.LineActionType = "A";
                stockLine.StockCode = item.Sku.Trim() == item.BatchCode.Trim() ? item.Sku : item.BatchCode;
                stockLine.OrderQty = item.Quantity;
                stockLine.OrderUom = item.UnitOfMeasure;
                stockLine.Price = item.PriceInclTax;
                stockLine.PriceUom = item.UnitOfMeasure;
                stockLine.AlwaysUsePriceEntered = "Y";
                stockLine.LineDiscPercent1 = "0.00";
                stockLine.LineDiscValue = "0.00";
                stockLine.LineDiscValFlag = "";
                stockLine.NonStockedLine = "";
                stockLine.NsProductClass = "";
                orderDetails.Add(stockLine);
            }

            // Convert to XML
            XElement salesOrdersXml = new XElement("SalesOrders",
                new XElement("Orders",
                    new XElement("OrderHeader",
                        new XElement("OrderActionType", "A"),
                        new XElement("Customer", erpRequest.AccountNumber),
                        new XElement("CustomerPoNumber", erpRequest.CustomerReference),
                        new XElement("CustomerName", erpRequest.AccountName),
                        new XElement("ShipAddress1", erpRequest.ShippingAddress.Address1),
                        new XElement("ShipAddress2", erpRequest.ShippingAddress.Address2),
                        new XElement("ShipAddress3", erpRequest.ShippingAddress.Address3),
                        new XElement("ShipAddress3Locality", ""),
                        new XElement("ShipAddress4", ""),
                        new XElement("ShipAddress5", ""),
                        new XElement("ShipPostalCode", erpRequest.ShippingAddress.ZipPostalCode),
                        new XElement("Email", erpRequest.CustomerEmail),
                        new XElement("OrderDiscPercent1", 0.00),
                        new XElement("OrderType", "W"),
                        new XElement("RequestedShipDate", erpRequest.DeliveryDate)
                    ),
                    new XElement("OrderDetails",
                        from detail in orderDetails
                        select new XElement("StockLine",
                            new XElement("LineActionType", "A"),
                            new XElement("AllocationAction", "B"),
                            new XElement("StockCode", detail.StockCode),
                            new XElement("Warehouse", detail.Warehouse),
                            new XElement("OrderQty", detail.OrderQty),
                            new XElement("OrderUom", detail.OrderUom),
                            new XElement("Price", detail.Price),
                            new XElement("PriceUom", detail.PriceUom),
                            new XElement("AlwaysUsePriceEntered", "Y"),
                            new XElement("LineDiscPercent1", "0.00"),
                            new XElement("LineDiscValue", "0.00"),
                            new XElement("LineDiscValFlag", ""),
                            new XElement("NonStockedLine", detail.NonStockedLine ?? string.Empty),
                            new XElement("NsProductClass", detail.NsProductClass ?? string.Empty)
                        )
                    )
                )
            );

            XElement salesParameterXml = new XElement("SalesOrders",
                new XElement("Parameters",
                    new XElement("Process", "IMPORT"),
                    new XElement("TypeOfOrder", "ORD"),
                    new XElement("OrderStatus", "1"),
                    new XElement("AllowNonStockItems", "Y"),
                    new XElement("AcceptOrdersIfNoCredit", "Y"),
                    new XElement("AcceptEarlierShipDate", "N"),
                    new XElement("AllowDuplicateOrderNumbers", "Y"),
                    new XElement("CheckForCustomerPoNumbers", "N"),
                    new XElement("AllowInvoiceInformationEntry", ""),
                    new XElement("AlwaysUsePriceEntered", ""),
                    new XElement("AllowZeroPrice", ""),
                    new XElement("AllowChangeToZeroPrice", ""),
                    new XElement("AddStockSalesOrderText", "N"),
                    new XElement("AddDangerousGoodsText", "N"),
                    new XElement("UseStockDescSupplied", ""),
                    new XElement("ValidateShippingInstrs", ""),
                    new XElement("AllocationAction", ""),
                    new XElement("IgnoreWarnings", "N"),
                    new XElement("AddAttachedServiceCharges", ""),
                    new XElement("StatusInProcess", ""),
                    new XElement("StatusInProcessResponse", ""),
                    new XElement("WarnIfCustomerOnHold", "N"),
                    new XElement("AcceptKitOptional", "N"),
                    new XElement("AllowBackOrderForPartialHold", ""),
                    new XElement("AllowBackOrderForSuperseded", ""),
                    new XElement("OverrideCustomerBackOrder", ""),
                    new XElement("UseMasterAccountForCustomerPartNo", ""),
                    new XElement("ApplyLeadTimeCalculation", ""),
                    new XElement("ApplyParentDiscountToComponents", "N"),
                    new XElement("AllowManualOrderNumberToBeUsed", "N")
                )
            );

            dynamic SqlRequest = new ExpandoObject();
            SqlRequest.action = "WCF";
            SqlRequest.Operator = "_Online";
            SqlRequest.operatorPassword = "Online";
            SqlRequest.company = string.IsNullOrWhiteSpace(erpRequest.Location) ? null : erpRequest.Location;
            SqlRequest.companyPassword = string.IsNullOrWhiteSpace(erpRequest.Location) ? "" : erpRequest.Location != "ZGPL" ? "" : "NOGO";
            SqlRequest.businessObject = "SORTOI";
            SqlRequest.method = "TransactionPost";
            SqlRequest.xmlIn = salesOrdersXml.ToString();
            SqlRequest.xmlParameters = salesParameterXml.ToString();

            return SqlRequest;
        }
        return null;
    }

    public async Task<dynamic> PrepareErpInvoiceHexRequestBody(ErpGetRequestModel erpRequest)
    {
        if (erpRequest != null)
        {
            dynamic queryRequest = new ExpandoObject();

            // Set the action and operator details
            queryRequest.action = "SRS";
            queryRequest.@operator = "_Online";
            queryRequest.operatorPassword = "Online";
            queryRequest.company = string.IsNullOrWhiteSpace(erpRequest.Location) ? null : erpRequest.Location;
            queryRequest.companyPassword = "";

            // Build the xmlIn
            XElement xmlIn = new XElement("Query",
                new XElement("Option",
                    new XElement("Function", "ONLINE"),
                    new XElement("DocumentType", "I"),
                    new XElement("Format", "1"),
                    new XElement("Reprint", "Y")
                ),
                new XElement("Filter",
                    new XElement("OrderNumber", new XAttribute("FilterType", "S"), new XAttribute("FilterValue", erpRequest.OrderNumber ?? string.Empty)),
                    new XElement("InvoiceNumber", new XAttribute("FilterType", "S"), new XAttribute("FilterValue", erpRequest.DocumentNumber ?? string.Empty))
                )
            );
            queryRequest.xmlIn = xmlIn.ToString();

            // Build the xmlParameters
            XElement xmlParameters = new XElement("DocumentControl",
                new XElement("DocumentType", "SalesOrder"),
                new XElement("Print", "False"),
                new XElement("Preview", "True"),
                new XElement("Email", "False"),
                new XElement("XmlOnly", "False"),
                new XElement("PrinterDetails",
                    new XElement("PrinterName", ""),
                    new XElement("PrintCopies", "1"),
                    new XElement("PrintCollate", "True")
                ),
                new XElement("EmailDetails",
                    new XElement("EmailFromAddress", ""),
                    new XElement("EmailToAddress", ""),
                    new XElement("EmailCcAddress", ""),
                    new XElement("EmailSubject", ""),
                    new XElement("EmailBodyText", "")
                )
            );
            queryRequest.xmlParameters = xmlParameters.ToString();

            return queryRequest;
        }
        return new ExpandoObject();
    }

    public async Task<dynamic> PrepareErpInvoiceRequestBody(ErpGetRequestModel erpRequest)
    {
        if (erpRequest != null)
        {
            var start = int.Parse(erpRequest.Start);

            dynamic SqlRequest = new ExpandoObject();
            SqlRequest.action = "SQL";
            SqlRequest.name = "Online_GetArMovement";

            dynamic args = new ExpandoObject();
            args.Company = string.IsNullOrWhiteSpace(erpRequest.Location) ? null : erpRequest.Location;
            args.Customer = string.IsNullOrWhiteSpace(erpRequest.AccountNumber) ? null : erpRequest.AccountNumber;
            args.PageNumber = start > 0 ? start : 1;
            erpRequest.Start = args.PageNumber.ToString();
            args.RowsPerPage = erpRequest.Limit > 0 ? erpRequest.Limit : 100;

            SqlRequest.args = args;

            return SqlRequest;
        }

        return new ExpandoObject();
    }

    public async Task<dynamic> PrepareErpStatementHexRequestBody(ErpGetRequestModel erpRequest)
    {
        if (erpRequest != null)
        {
            dynamic queryRequest = new ExpandoObject();

            queryRequest.action = "SRS";
            queryRequest.@operator = "_Online";
            queryRequest.operatorPassword = "Online";
            queryRequest.company = string.IsNullOrWhiteSpace(erpRequest.Location) ? null : erpRequest.Location;
            queryRequest.companyPassword = "";

            // Build the xmlIn
            XElement xmlIn = new XElement("Query",
                new XElement("Option",
                    new XElement("Function", "ONLINE"),
                    new XElement("Format", "1"),
                    new XElement("StatementAsOf", "C"),
                    new XElement("StatementDate", erpRequest.DateFrom?.ToString("yyyy-MM-dd") ?? ""),
                    new XElement("StatementAgeing", "S"),
                    new XElement("BalanceType", "A"),
                    new XElement("MinimumBalance", "0.00")
                ),
                new XElement("Filter",
                    new XElement("Customer",
                        new XAttribute("FilterType", "S"),
                        new XAttribute("FilterValue", erpRequest.AccountNumber ?? string.Empty))
                )
            );
            queryRequest.xmlIn = xmlIn.ToString();

            // Build the xmlParameters
            XElement xmlParameters = new XElement("DocumentControl",
                new XElement("DocumentType", "AR Statement Print"),
                new XElement("Print", "False"),
                new XElement("Preview", "True"),
                new XElement("Email", "False"),
                new XElement("XmlOnly", "False"),
                new XElement("PrinterDetails",
                    new XElement("PrinterName", ""),
                    new XElement("PrintCopies", "1"),
                    new XElement("PrintCollate", "True")
                ),
                new XElement("EmailDetails",
                    new XElement("EmailFromAddress", ""),
                    new XElement("EmailToAddress", ""),
                    new XElement("EmailCcAddress", ""),
                    new XElement("EmailSubject", ""),
                    new XElement("EmailBodyText", "")
                )
            );
            queryRequest.xmlParameters = xmlParameters.ToString();

            return queryRequest;
        }
        return new ExpandoObject();
    }

    public async Task<dynamic> PrepareErpDealsRequestBody(ErpGetRequestModel erpRequest)
    {
        if (erpRequest != null)
        {
            var start = int.Parse(erpRequest.Start);

            dynamic SqlRequest = new ExpandoObject();
            SqlRequest.action = "SQL";
            SqlRequest.name = "Online_GetDeals";

            dynamic args = new ExpandoObject();
            args.Company = string.IsNullOrWhiteSpace(erpRequest.Location) ? null : erpRequest.Location;
            args.PageNumber = start > 0 ? start : 1;
            args.RowsPerPage = erpRequest.Limit > 0 ? erpRequest.Limit : 100;

            SqlRequest.args = args;
            erpRequest.Start = args.PageNumber.ToString();

            return SqlRequest;
        }

        return new ExpandoObject();
    }

    public async Task<dynamic> PrepareErpOrdersRequestBody(ErpGetRequestModel erpRequest)
    {
        if (erpRequest != null)
        {
            var start = int.Parse(erpRequest.Start);

            dynamic SqlRequest = new ExpandoObject();
            SqlRequest.action = "SQL";
            SqlRequest.name = "Online_GetOrderDetail";

            dynamic args = new ExpandoObject();
            args.Company = string.IsNullOrWhiteSpace(erpRequest.Location) ? null : erpRequest.Location;
            args.AccountNumber = string.IsNullOrWhiteSpace(erpRequest.AccountNumber) ? null : erpRequest.AccountNumber;
            args.SalesOrder = string.IsNullOrWhiteSpace(erpRequest.OrderNumber) ? null : erpRequest.OrderNumber;
            args.PageNumber = start > 0 ? start : 1;
            args.RowsPerPage = erpRequest.Limit > 0 ? erpRequest.Limit : 100;

            SqlRequest.args = args;
            erpRequest.Start = args.PageNumber.ToString();

            return SqlRequest;
        }

        return new ExpandoObject();
    }

    public (bool IsValid, string ErrorMessage) ValidateAdditionalHardCodedValuesSettings(string jsonSetting)
    {
        if (string.IsNullOrEmpty(jsonSetting))
            return (true, string.Empty);

        List<AdditionalValues> settings;
        try
        {
            settings = JsonConvert.DeserializeObject<List<AdditionalValues>>(jsonSetting);
        }
        catch (JsonException)
        {
            return (false, "Invalid JSON format for hard-coded values. Please provide values in a proper json format.");
        }

        foreach (var setting in settings)
        {
            if (setting is null)
                return (false, "Invalid JSON format for hard-coded values. Please provide values in a proper json format.");

            // Key Validation
            if (string.IsNullOrWhiteSpace(setting.Key))
                return (false, "Each hard-coded value must have a non-empty 'key' that is not whitespace.");

            // Type Validation
            if (string.IsNullOrEmpty(setting.Type))
                return (false, $"In hard-coded values, type is required for key '{setting.Key}'");

            setting.Type = setting.Type.Trim();
            if (!_validTypes.Contains(setting.Type.ToLowerInvariant()))
                return (false, $"In hard-coded values, invalid type found for key '{setting.Key}'. Valid types: string, int, bool, decimal, datetime");
        }

        return (true, string.Empty);
    }
}