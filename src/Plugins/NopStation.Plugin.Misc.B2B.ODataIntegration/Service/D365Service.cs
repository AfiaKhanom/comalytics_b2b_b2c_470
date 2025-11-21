using System.Dynamic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nop.Core;
using Nop.Services.Configuration;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.Misc.B2B.ODataIntegration.Areas.Admin.Model;
using NopStation.Plugin.Misc.B2B.ODataIntegration.GetRequestSettings;
using NopStation.Plugin.Misc.B2B.ODataIntegration.Model;
using NopStation.Plugin.Misc.B2B.ODataIntegration.Settings;
using NopStation.Plugin.Misc.B2B.ODataIntegration.Settings.PlaceOrderSettings;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.Services
{
    public class D365Service : ID365Service
    {
        #region Fields

        private readonly D365HttpClient _d365HttpClient;
        private readonly IErpLogsService _erpLogsService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;

        private readonly List<KeyValuePair<string, string>> _additionalMappings = new();
        private readonly List<KeyValuePair<string, string?>> _productAttributeMappings = new();
        private string _dateTimeFormat;

        private static readonly HashSet<string> _validTypes = new HashSet<string>
        {
            "string", "int", "bool", "decimal", "datetime", "double"
        };

        private const int ACCOUNT_NUMBER_LIMIT = 20;
        private const int ACCOUNT_NAME_LIMIT = 100;
        private const int CUSTOMER_NAME_LIMIT = 100;
        private const int CUSTOMER_EMAIL_LIMIT = 100;
        private const int CUSTOMER_FIRST_NAME_LIMIT = 100;
        private const int CUSTOMER_LAST_NAME_LIMIT = 100;
        private const int CUSTOMER_PHONE_NUMBER_LIMIT = 20;
        private const int CUSTOMER_MOBILE_NUMBER_LIMIT = 20;
        private const int ERP_ADDRESS_NAME_LIMIT = 100;
        private const int ERP_ADDRESS_ADDRESS1_LIMIT = 100;
        private const int ERP_ADDRESS_ADDRESS2_LIMIT = 50;
        private const int ERP_ADDRESS_ADDRESS3_LIMIT = 50;
        private const int ERP_ADDRESS_CITY_LIMIT = 30;
        private const int ERP_ADDRESS_STATE_PROVINCE_LIMIT = 30;
        private const int ERP_ADDRESS_COUNTRY_LIMIT = 10;
        private const int ERP_ADDRESS_ZIPPOSTALCODE_LIMIT = 20;
        private const int SKU_LIMIT = 20;
        private const int DESCRIPTION_LIMIT = 100;
        private const int UNIT_OF_MEASURE_LIMIT = 10;

        #endregion

        #region Ctor

        public D365Service(
            D365HttpClient d365HttpClient,
            IErpLogsService erpLogsService,
            IStoreContext storeContext,
            ISettingService settingService)
        {
            _d365HttpClient = d365HttpClient;
            _erpLogsService = erpLogsService;
            _storeContext = storeContext;
            _settingService = settingService;
        }

        #endregion

        #region Utils

        private async Task LoadDateTimeFormat()
        {
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<D365IntegrationSettings>(storeScope);
            _dateTimeFormat = settings.DefaultDateTimeFormat;
        }

        private async Task LoadAdditionalMappings()
        {
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var additionalSettings = await _settingService.LoadSettingAsync<D365IntegrationSettings>(storeScope);

            TranslationTableModel? mappings;
            try
            {
                mappings = JsonConvert.DeserializeObject<TranslationTableModel>(additionalSettings.AdditionalMappings);
            }
            catch (JsonException)
            {
                return;
            }

            if (mappings == null)
                return;

            if (mappings.OrderTypes != null && mappings.OrderTypes.Any())
            {
                foreach (var item in mappings.OrderTypes)
                {
                    if (item == null)
                        continue;

                    if (!string.IsNullOrWhiteSpace(item.Key) && !string.IsNullOrWhiteSpace(item.Value))
                    {
                        AddOrUpdateAdditionalMapping(item.Key.Trim(), item.Value.Trim());
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
                        AddOrUpdateAdditionalMapping(item.Key.Trim(), item.Value.Trim());
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
                        AddOrUpdateAdditionalMapping(item.Key.Trim(), item.Value.Trim());
                    }
                }
            }
            return;
            

        }
        public async Task LoadProductAttributeMappings()
        {
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var productSettings = await _settingService.LoadSettingAsync<ErpProductSettings>(storeScope);

            List<ErpProductAttributeModel>? attributeMappings;

            try
            {
                attributeMappings = JsonConvert.DeserializeObject<List<ErpProductAttributeModel>>(productSettings.ProductAttributes);
            }
            catch (JsonException)
            {
                return;
            }

            if (attributeMappings == null || !attributeMappings.Any())
                return;

            _productAttributeMappings.Clear();

            foreach (var item in attributeMappings)
            {
                if (item == null)
                    continue;

                if (!string.IsNullOrWhiteSpace(item.Key) && !string.IsNullOrWhiteSpace(item.Value))
                {
                    AddProductAttributeMapping(item.Key.Trim(), item.Value.Trim());
                }
            }
        }
        private (string Key, string Value) ExtractKeyValue(string input)
        {
            var parts = input.Split(':');
            if (parts.Length > 1)
            {
                var key = parts[0].Trim();
                var value = parts[1].Trim().Trim('"');
                return (key, value);
            }
            return (string.Empty, string.Empty);
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

        private string ConvertToDate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var date = DateTime.Parse(value, null, System.Globalization.DateTimeStyles.RoundtripKind);
            return date.ToString("yyyy-MM-dd");
        }

        private string ConvertToString(object value, Type type)
        {
            if (value == null)
                return null;

            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            if (underlyingType == typeof(DateTime))
                return ((DateTime)value).ToString(_dateTimeFormat);

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
        private IDictionary<string, object> MergeDictionaries(IDictionary<string, object> first, IDictionary<string, object> second)
        {
            foreach (var kvp in second)
            {
                first[kvp.Key] = kvp.Value;
            }
            return first;
        }
        public bool IsValidD365IntegrationApiUrlSettings(D365IntegrationApiUrlSettings apiUrlSettings, string requestUrl)
        {
            if (string.IsNullOrWhiteSpace(apiUrlSettings.AccessTokenUrl) ||
                string.IsNullOrWhiteSpace(apiUrlSettings.ClientId) ||
                string.IsNullOrWhiteSpace(apiUrlSettings.ClientSecret) ||
                string.IsNullOrWhiteSpace(apiUrlSettings.Scope) ||
                string.IsNullOrWhiteSpace(requestUrl))
            {
                return false;
            }

            return true;
        }
        private async Task<string> PrepareQueryParams<TSettings>(ErpGetRequestModel erpRequest, TSettings settings, List<AdditionalValues> additionalFilters = null)
        {
            var queryParams = new List<string>();

            var skip = string.IsNullOrWhiteSpace(erpRequest.Start) ? "0" : erpRequest.Start;
            var top = erpRequest.Limit > 0 ? erpRequest.Limit.ToString() : "10";

            queryParams.Add($"$top={top}");
            queryParams.Add($"$skip={skip}");

            var filters = new List<string>();

            string? GetFieldName(string propertyName)
            {
                if (string.IsNullOrWhiteSpace(propertyName))
                    return string.Empty;

                var property = typeof(TSettings).GetProperty(propertyName);
                if (property == null)
                    return string.Empty;

                var value = property.GetValue(settings);
                return value?.ToString() ?? string.Empty;
            }

            void AddFilter(string propertyName, object? value, string operatorSymbol = "eq", string format = "")
            {
                var settingValue = GetFieldName(propertyName);

                if (!string.IsNullOrWhiteSpace(settingValue))
                    settingValue.Trim();

                if (!string.IsNullOrWhiteSpace(settingValue) && value != null)
                {
                    var formattedValue = "";
                    if (!string.IsNullOrWhiteSpace(format) && value is DateTime dt)
                    {
                        formattedValue = dt.ToString(format);
                    }
                    else
                    {
                        formattedValue = $"'{value}'";
                    }

                    filters.Add($"{settingValue} {operatorSymbol} {formattedValue}");
                }
            }

            if (!string.IsNullOrWhiteSpace(erpRequest.AccountNumber))
                AddFilter(nameof(erpRequest.AccountNumber), erpRequest.AccountNumber);

            if (!string.IsNullOrWhiteSpace(erpRequest.Location))
                AddFilter(nameof(erpRequest.Location), erpRequest.Location);

            if (!string.IsNullOrWhiteSpace(erpRequest.WarehouseCode))
                AddFilter(nameof(erpRequest.WarehouseCode), erpRequest.WarehouseCode);

            if (!string.IsNullOrWhiteSpace(erpRequest.DocumentNumber))
                AddFilter(nameof(erpRequest.DocumentNumber), erpRequest.DocumentNumber);

            if (!string.IsNullOrWhiteSpace(erpRequest.OrderNumber))
                AddFilter(nameof(erpRequest.OrderNumber), erpRequest.OrderNumber);

            if (!string.IsNullOrWhiteSpace(erpRequest.ProductSku))
                AddFilter(nameof(erpRequest.ProductSku), erpRequest.ProductSku);

            if (!string.IsNullOrWhiteSpace(erpRequest.PriceCode))
                AddFilter(nameof(erpRequest.PriceCode), erpRequest.PriceCode);

            if (erpRequest.LastChangedDate.HasValue)
                AddFilter(nameof(erpRequest.LastChangedDate), erpRequest.LastChangedDate, "ge", _dateTimeFormat);

            if (erpRequest.DateFrom.HasValue)
                AddFilter(nameof(erpRequest.DateFrom), erpRequest.DateFrom, "le", _dateTimeFormat);

            if (erpRequest.DateTo.HasValue)
                AddFilter(nameof(erpRequest.DateTo), erpRequest.DateTo, "ge", _dateTimeFormat);

            if (additionalFilters != null)
            {
                if (additionalFilters != null && additionalFilters.Any())
                {
                    foreach (var filter in additionalFilters)
                    {
                        if (string.IsNullOrWhiteSpace(filter.Key) || string.IsNullOrWhiteSpace(filter.Value))
                            continue;

                        var operatorSymbol = "eq";
                        var format = "";
                        object? parsedValue = filter.Value;

                        switch (filter.Type?.ToLowerInvariant())
                        {
                            case "string":
                                parsedValue = $"'{filter.Value}'";
                                break;

                            case "int":
                                if (int.TryParse(filter.Value, out var intVal))
                                    parsedValue = intVal;
                                break;

                            case "double":
                                if (double.TryParse(filter.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var doubleVal))
                                    parsedValue = doubleVal;
                                break;

                            case "decimal":
                                if (decimal.TryParse(filter.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var decimalVal))
                                    parsedValue = decimalVal;
                                break;

                            case "bool":
                                if (bool.TryParse(filter.Value, out var boolVal))
                                    parsedValue = boolVal.ToString().ToLower();
                                break;

                            case "datetime":
                            case "datetimeoffset":
                            case "datetimekind":
                                if (DateTime.TryParse(filter.Value, out var dtVal))
                                {
                                    parsedValue = dtVal.ToString(_dateTimeFormat);
                                    operatorSymbol = "gt";
                                }
                                break;

                            default:
                                parsedValue = $"'{filter.Value}'";
                                break;
                        }

                        filters.Add($"{filter.Key} {operatorSymbol} {parsedValue}");
                    }
                }

            }

            if (filters.Any())
                queryParams.Add($"$filter={string.Join(" and ", filters)}");

            return "?" + string.Join("&", queryParams);
        }
        private dynamic PrepareRequestBody(ErpGetRequestModel erpRequest, ErpInvoicePdfGetRequestSettings settings)
        {
            dynamic request = new ExpandoObject();
            var dict = (IDictionary<string, object>)request;

            if (erpRequest == null || settings == null)
                return request;

            if (!string.IsNullOrWhiteSpace(settings.AccountNumber) && !string.IsNullOrWhiteSpace(erpRequest.AccountNumber))
                dict[settings.AccountNumber] = erpRequest.AccountNumber;

            if (!string.IsNullOrWhiteSpace(settings.DocumentNumber) && !string.IsNullOrWhiteSpace(erpRequest.DocumentNumber))
                dict[settings.DocumentNumber] = erpRequest.DocumentNumber;

            if (!string.IsNullOrWhiteSpace(settings.OrderNumber) && !string.IsNullOrWhiteSpace(erpRequest.OrderNumber))
                dict[settings.OrderNumber] = erpRequest.OrderNumber;

            if (!string.IsNullOrWhiteSpace(settings.ProductSku) && !string.IsNullOrWhiteSpace(erpRequest.ProductSku))
                dict[settings.ProductSku] = erpRequest.ProductSku;

            if (!string.IsNullOrWhiteSpace(settings.Location) && !string.IsNullOrWhiteSpace(erpRequest.Location))
                dict[settings.Location] = erpRequest.Location;

            if (!string.IsNullOrWhiteSpace(settings.PriceCode) && !string.IsNullOrWhiteSpace(erpRequest.PriceCode))
                dict[settings.PriceCode] = erpRequest.PriceCode;

            if (!string.IsNullOrWhiteSpace(settings.LastChangedDate) && erpRequest.LastChangedDate.HasValue)
                dict[settings.LastChangedDate] = erpRequest.LastChangedDate.Value.ToString(_dateTimeFormat);

            return request;
        }
        private string RemoveQueryString(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return url;

            int index = url.LastIndexOf('?');
            return index >= 0 ? url.Substring(0, index) : url;
        }

        private string GetExpandParam(string originalUrl)
        {
            string expandParam = null;
            int expandIndex = originalUrl.IndexOf("$expand=", StringComparison.OrdinalIgnoreCase);
            if (expandIndex >= 0)
            {
                int endIndex = originalUrl.IndexOf('&', expandIndex + 1);
                if (endIndex == -1)
                    endIndex = originalUrl.Length;

                expandParam = $"&{originalUrl.Substring(expandIndex, endIndex - expandIndex)}";
            }
            return expandParam;
        }
        private async Task<string> PrepareUrl<TSettings>(ErpGetRequestModel erpRequest, TSettings settings, string fullUrl = "", int limit = 0, string additionalFiltersSetting = "")
        {
            if (erpRequest == null || settings == null)
                return string.Empty;

            var endpoint = "";
            List<AdditionalValues> additionalFilters;

            erpRequest.Limit = limit;
            var queryParams = "";

            if (!string.IsNullOrWhiteSpace(additionalFiltersSetting))
            {
                additionalFilters = JsonConvert.DeserializeObject<List<AdditionalValues>>(additionalFiltersSetting);
                queryParams = await PrepareQueryParams(erpRequest, settings, additionalFilters);
            }
            else
            {
                queryParams = await PrepareQueryParams(erpRequest, settings);
            }
            endpoint = $"{RemoveQueryString(fullUrl)}{queryParams}{GetExpandParam(fullUrl)}";

            return endpoint;
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
                "double" => double.TryParse(value, out var db) ? db : null,
                "string" => value,
                _ => value // fallback
            };
        }

        public Dictionary<string, object> ParseHardcodedValuesFromSetting<TSettings>(string jsonSetting, TSettings settings)// where TSettings : ISettings
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
                // please put a log with stack trace
            }

            return dictionary;
        }

        private async Task<IDictionary<string, object>> AddShippingCostItem(decimal price)
        {
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<D365IntegrationSettings>(storeScope);
            return await ConvertJsonStringToDictionary(settings.ShippingCostItemPayload, price);
        }
        private async Task<IDictionary<string, object>> ConvertJsonStringToDictionary(string jsonString, decimal price)
        {
            if (string.IsNullOrWhiteSpace(jsonString))
                return new Dictionary<string, object>();

            try
            {
                var updatedJson = jsonString.Replace("{0}", price.ToString());
                var jObject = JObject.Parse(updatedJson);
                var result = new Dictionary<string, object>();

                foreach (var property in jObject.Properties())
                {
                    result[property.Name] = ConvertJTokenToObject(property.Value);
                }

                return result;
            }
            catch (Exception ex)
            {
                await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Information, ErpSyncLevel.Order, "Could not parse shipping cost item", ex.Message);
                return new Dictionary<string, object>();
            }
        }

        private object ConvertJTokenToObject(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.String:
                    return token.Value<string>();
                case JTokenType.Integer:
                    return token.Value<long>();
                case JTokenType.Float:
                    return token.Value<decimal>();
                case JTokenType.Boolean:
                    return token.Value<bool>();
                case JTokenType.Date:
                    return token.Value<DateTime>();
                case JTokenType.Null:
                    return null;
                case JTokenType.Array:
                    return token.ToObject<object[]>();
                case JTokenType.Object:
                    return token.ToObject<Dictionary<string, object>>();
                default:
                    return token.ToString();
            }
        }
        private async Task<IDictionary<string, object>> PrepareErpOrderRequestBodyAsync<TRequest, TSettings>(TRequest erpRequest, TSettings settings, string orderNo = "", string documentType = "")
        {
            var dict = new Dictionary<string, object>();

            if (erpRequest == null || settings == null)
                return dict;

            var requestProps = erpRequest.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var settingsProps = settings.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            if (!string.IsNullOrWhiteSpace(orderNo))
                dict["Document_No"] = orderNo;
            if (!string.IsNullOrWhiteSpace(documentType))
                dict["Document_Type"] = documentType;

            foreach (var prop in settingsProps)
            {
                var settingValue = prop.GetValue(settings) as string;

                if (string.IsNullOrWhiteSpace(settingValue))
                    continue;

                // for HardcodedPayloadParamsJson, parse the JSON and merge it into the dictionary
                if (prop.Name.ToLowerInvariant().Equals("AdditionalHardCodedValues".ToLowerInvariant()))
                {
                    var jsonParams = ParseHardcodedValuesFromSetting(settingValue, settings);
                    foreach (var kvp in jsonParams)
                    {
                        if (!dict.ContainsKey(kvp.Key))
                            dict[kvp.Key] = kvp.Value;
                    }
                    continue;
                }

                if (settingValue.Contains(":"))
                {
                    var pair = ExtractKeyValue(settingValue);
                    dict[pair.Key] = pair.Value;
                }
                else
                {
                    var matchingProp = requestProps.FirstOrDefault(p => p.Name == prop.Name);
                    if (matchingProp == null)
                        continue;

                    var value = matchingProp.GetValue(erpRequest);
                    if (value == null)
                        continue;

                    if (IsSimpleType(matchingProp.PropertyType))
                    {
                        var val = ConvertToString(value, matchingProp.PropertyType);

                        if (matchingProp.Name == nameof(ErpPlaceOrderDataModel.OrderDate) ||
                           matchingProp.Name == nameof(ErpPlaceOrderDataModel.DeliveryDate) ||
                           matchingProp.Name == nameof(ErpPlaceOrderDataModel.DateRequired))
                        {
                            val = ConvertToDate(val);
                        }
                        dict[settingValue] = val;
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
            }

            return dict;
        }

        private async Task<IDictionary<string, object>> PrepareErpCreateRequestBodyAsync<TRequest, TSettings>(TRequest erpRequest, TSettings settings)
        {
            var dict = new Dictionary<string, object>();

            if (erpRequest == null || settings == null)
                return dict;

            var requestProps = erpRequest.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var settingsProps = settings.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in settingsProps)
            {
                var settingValue = prop.GetValue(settings) as string;

                if (string.IsNullOrWhiteSpace(settingValue))
                    continue;
                if (prop.Name.ToLowerInvariant().Equals("AdditionalHardCodedValues".ToLowerInvariant()))
                {
                    var jsonParams = ParseHardcodedValuesFromSetting(settingValue, settings);
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
        private async Task<dynamic> PrepareErpOrderItemRequestBodyAsync(ErpPlaceOrderDataModel erpRequest, ErpPlaceOrderItemSettings settings, string orderNo, string documentType, List<int> orderItemIds = null)
        {
            var items = erpRequest.ErpPlaceOrderItemDatas;
            var requests = new List<object>();

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];

                var request = new
                {
                    method = "POST",
                    id = orderItemIds != null && i < orderItemIds.Count
                         ? orderItemIds[i].ToString()
                         : (i + 1).ToString(),

                    url = "Company('APG')/COML_Sales_Orders_Lines",
                    headers = new
                    {
                        Content_Type = "application/json"
                    },
                    body = await PrepareErpOrderRequestBodyAsync(item, settings, orderNo, documentType)
                };

                requests.Add(request);
            }

            #region Add Shopping Cost Item
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var integrationSettings = await _settingService.LoadSettingAsync<D365IntegrationSettings>(storeScope);

            if (!string.IsNullOrWhiteSpace(integrationSettings.ShippingCostItemPayload))
            {
                var shoppingCostItem = await AddShippingCostItem(erpRequest.ShippingAmount);

                var shippingCostItem = new
                {
                    method = "POST",
                    id = "0",
                    url = "Company('APG')/COML_Sales_Orders_Lines",
                    headers = new
                    {
                        Content_Type = "application/json"
                    },
                    body = shoppingCostItem
                };

                requests.Add(shippingCostItem);
            }

            #endregion
            var payload = new { requests };

            return payload;

        }

        private string FindKeyByValue(string value)
        {
            return _additionalMappings
                .FirstOrDefault(kvp => string.Equals(kvp.Value, value, StringComparison.OrdinalIgnoreCase))
                .Key ?? string.Empty;
        }

        private async Task<dynamic> PreparePlaceOrderBodyAsync(
            ErpPlaceOrderDataModel erpRequest,
            ErpPlaceOrderSettings orderSettings,
            ErpPlaceOrderItemSettings itemSettings,
            ErpPlaceOrderShippingAddressSettings shippingAddressSettings,
            ErpPlaceOrderBillingAddressSettings billingAddressSettings)
        {
            var orderDict = await PrepareErpOrderRequestBodyAsync(erpRequest, orderSettings);
            var shippingDict = await PrepareErpOrderRequestBodyAsync(erpRequest.ShippingAddress, shippingAddressSettings);
            var billingDict = await PrepareErpOrderRequestBodyAsync(erpRequest.BillingAddress, billingAddressSettings);

            dynamic orderDetails = new ExpandoObject();
            var orderDetailsDict = (IDictionary<string, object>)orderDetails;

            void Merge(IDictionary<string, object> source)
            {
                foreach (var kvp in source)
                {
                    orderDetailsDict[kvp.Key] = kvp.Value;
                }
            }

            Merge(orderDict);
            Merge(shippingDict);
            Merge(billingDict);

            #region replace values with transtation table

            var literals = new[]{ "DELIVERY", "COLLECT", nameof(ErpOrderType.B2BSalesOrder), nameof(ErpOrderType.B2CSalesOrder), nameof(ErpOrderType.B2BQuote), nameof(ErpOrderType.B2BQuote)};

            string findKeyByValueFromModel(string value)
            {
                return orderDetailsDict
                    .FirstOrDefault(kvp => string.Equals(kvp.Value?.ToString(), value, StringComparison.OrdinalIgnoreCase))
                    .Key ?? string.Empty;
            }

            foreach (var literal in literals)
            {

                var key = findKeyByValueFromModel(literal);
                if (orderDetailsDict.ContainsKey(key))
                {
                    var toReplace = FindKeyByValue(literal);
                    if (!string.IsNullOrWhiteSpace(toReplace))
                    {
                        orderDetailsDict[key] = toReplace;
                    }
                }
            }
            #endregion

            // Add orderlines if needed
            var orderLines = new List<object>();

            if (erpRequest.ErpPlaceOrderItemDatas != null && erpRequest.ErpPlaceOrderItemDatas.Any())
            {
                foreach (var item in erpRequest.ErpPlaceOrderItemDatas)
                {
                    var itemDict = await PrepareErpOrderRequestBodyAsync(item, itemSettings);
                    orderLines.Add(itemDict);
                }
                //Adding Shipping cost item

                var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                var integrationSettings = await _settingService.LoadSettingAsync<D365IntegrationSettings>(storeScope);

                if (!string.IsNullOrWhiteSpace(integrationSettings.ShippingCostItemPayload))
                {
                    var shoppingCostItem = await AddShippingCostItem(erpRequest.ShippingAmount);
                    orderLines.Add(shoppingCostItem);
                }

                orderDetailsDict["orderlines"] = orderLines;
            }

            return orderDetails;
        }



        private static Dictionary<string, string?> ToKeyValueDictionary(object obj)
        {
            return obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => new { p.Name, Value = p.GetValue(obj)?.ToString() })
                .Where(p => !string.IsNullOrWhiteSpace(p.Value))
                .ToDictionary(p => p.Name, p => p.Value);
        }

        private static Dictionary<string, string?> ToKeyValueDictionary(object obj, int idx)
        {
            return obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(prop =>
                {
                    string? value = prop.GetValue(obj)?.ToString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        int firstStarIndex = value.IndexOf("[*]");
                        if (firstStarIndex >= 0)
                        {
                            value = value.Remove(firstStarIndex, 3)
                                         .Insert(firstStarIndex, $"[{idx}]");
                        }
                        return new { prop.Name, Value = (string?)value };
                    }
                    return null;
                })
                .Where(p => p != null)
                .ToDictionary(p => p!.Name, p => p.Value);
        }


        private static Dictionary<string, string?> ToKeyValueDictionary(object obj, string prefix)
        {
            var section = prefix.Split('.').Last();

            return obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(prop =>
                {
                    var value = prop.GetValue(obj)?.ToString();
                    return new { Key = $"{section}.{prop.Name}", Value = value };
                })
                .Where(p => !string.IsNullOrWhiteSpace(p.Value))
                .ToDictionary(p => p.Key, p => p.Value);
        }


        private List<KeyValuePair<string, string?>> ExtractKeyValuePairs(string input)
        {
            var result = new List<KeyValuePair<string, string?>>();

            if (string.IsNullOrWhiteSpace(input))
                return result;

            input = input.Trim('[', ']');

            var regex = new Regex("\"(.*?)\":\"(.*?)\"");
            var matches = regex.Matches(input);

            foreach (Match match in matches)
            {
                var key = match.Groups[1].Value;
                var value = match.Groups[2].Value;
                result.Add(new KeyValuePair<string, string?>(key, value));
            }

            return result;
        }

        private string InsertIndex(string value, int idx)
        {
            if (!string.IsNullOrEmpty(value))
            {
                int firstStarIndex = value.IndexOf("[*]");
                if (firstStarIndex >= 0)
                {
                    value = value.Remove(firstStarIndex, 3)
                          .Insert(firstStarIndex, $"[{idx}]");
                }
            }
            return value;
        }

        private void LimitCharacters(ErpPlaceOrderDataModel erpRequest)
        {
            string trim(string value, int limit)
            {
                if (string.IsNullOrWhiteSpace(value))
                    return value;

                return value.Length > limit ? value.Substring(0, limit) : value;
            }

            erpRequest.AccountNumber = trim(erpRequest.AccountNumber, ACCOUNT_NUMBER_LIMIT);
            erpRequest.AccountName = trim(erpRequest.AccountName, ACCOUNT_NAME_LIMIT);
            erpRequest.CustomerName = trim(erpRequest.CustomerName, CUSTOMER_NAME_LIMIT);
            erpRequest.CustomerEmail = trim(erpRequest.CustomerEmail, CUSTOMER_EMAIL_LIMIT);
            erpRequest.CustomerFirstName = trim(erpRequest.CustomerFirstName, CUSTOMER_FIRST_NAME_LIMIT);
            erpRequest.CustomerLastName = trim(erpRequest.CustomerLastName, CUSTOMER_LAST_NAME_LIMIT);
            erpRequest.CustomerPhoneNumber = trim(erpRequest.CustomerPhoneNumber, CUSTOMER_PHONE_NUMBER_LIMIT);
            erpRequest.CustomerMobileNumber = trim(erpRequest.CustomerMobileNumber, CUSTOMER_MOBILE_NUMBER_LIMIT);

            if (erpRequest.ShippingAddress != null)
            {
                erpRequest.ShippingAddress.Name = trim(erpRequest.ShippingAddress.Name, ERP_ADDRESS_NAME_LIMIT);
                erpRequest.ShippingAddress.Address1 = trim(erpRequest.ShippingAddress.Address1, ERP_ADDRESS_ADDRESS1_LIMIT);
                erpRequest.ShippingAddress.Address2 = trim(erpRequest.ShippingAddress.Address2, ERP_ADDRESS_ADDRESS2_LIMIT);
                erpRequest.ShippingAddress.Address3 = trim(erpRequest.ShippingAddress.Address3, ERP_ADDRESS_ADDRESS3_LIMIT);
                erpRequest.ShippingAddress.City = trim(erpRequest.ShippingAddress.City, ERP_ADDRESS_CITY_LIMIT);
                erpRequest.ShippingAddress.StateProvince = trim(erpRequest.ShippingAddress.StateProvince, ERP_ADDRESS_STATE_PROVINCE_LIMIT);
                erpRequest.ShippingAddress.Country = trim(erpRequest.ShippingAddress.Country, ERP_ADDRESS_COUNTRY_LIMIT);
                erpRequest.ShippingAddress.ZipPostalCode = trim(erpRequest.ShippingAddress.ZipPostalCode, ERP_ADDRESS_ZIPPOSTALCODE_LIMIT);
            }

            if (erpRequest.BillingAddress != null)
            {
                erpRequest.BillingAddress.Name = trim(erpRequest.BillingAddress.Name, ERP_ADDRESS_NAME_LIMIT);
                erpRequest.BillingAddress.Address1 = trim(erpRequest.BillingAddress.Address1, ERP_ADDRESS_ADDRESS1_LIMIT);
                erpRequest.BillingAddress.Address2 = trim(erpRequest.BillingAddress.Address2, ERP_ADDRESS_ADDRESS2_LIMIT);
                erpRequest.BillingAddress.Address3 = trim(erpRequest.BillingAddress.Address3, ERP_ADDRESS_ADDRESS3_LIMIT);
                erpRequest.BillingAddress.City = trim(erpRequest.BillingAddress.City, ERP_ADDRESS_CITY_LIMIT);
                erpRequest.BillingAddress.StateProvince = trim(erpRequest.BillingAddress.StateProvince, ERP_ADDRESS_STATE_PROVINCE_LIMIT);
                erpRequest.BillingAddress.Country = trim(erpRequest.BillingAddress.Country, ERP_ADDRESS_COUNTRY_LIMIT);
                erpRequest.BillingAddress.ZipPostalCode = trim(erpRequest.BillingAddress.ZipPostalCode, ERP_ADDRESS_ZIPPOSTALCODE_LIMIT);
            }

            if (erpRequest.ErpPlaceOrderItemDatas != null)
            {
                foreach (var item in erpRequest.ErpPlaceOrderItemDatas)
                {
                    item.Sku = trim(item.Sku, SKU_LIMIT);
                    item.Description = trim(item.Description, DESCRIPTION_LIMIT);
                    item.UnitOfMeasure = trim(item.UnitOfMeasure, UNIT_OF_MEASURE_LIMIT);
                }
            }
        }

        #endregion

        #region Methods

        public void AddOrUpdateAdditionalMapping(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            key = key.Trim();
            value = value?.Trim() ?? string.Empty;

            _additionalMappings.Add(new KeyValuePair<string, string>(key, value));
        }


        public void AddProductAttributeMapping(string key, string? value)
        {
            _productAttributeMappings.Add(new KeyValuePair<string, string?>(key, value));
        }
        public void AddOrUpdateDefaultDateTimeFormat(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            _dateTimeFormat = value;
        }

        #region Products
        public async Task<ErpResponseData<IList<ErpProductDataModel>>> GetProductsFromErpAsync(ErpGetRequestModel erpRequest)
        {
            var erpResponseData = new ErpResponseData<IList<ErpProductDataModel>>();
            var responseContent = string.Empty;

            try
            {
                if (erpRequest == null)
                {
                    erpResponseData.ErpResponseModel.IsError = false;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = "Request body content no data";
                    return erpResponseData;
                }
                await LoadDateTimeFormat();
                var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                var apiUrlSettings = await _settingService.LoadSettingAsync<D365IntegrationApiUrlSettings>(storeScope);

                if (!IsValidD365IntegrationApiUrlSettings(apiUrlSettings, apiUrlSettings.OrderApiUrl))
                {
                    erpResponseData.ErpResponseModel.IsError = true;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = "BCS Integration Settings is not configured.";
                    return erpResponseData;
                }

                var productSettings = await _settingService.LoadSettingAsync<ErpProductSettings>(storeScope);
                var erpProductGetRequestSettings = await _settingService.LoadSettingAsync<ErpProductGetRequestSettings>(storeScope);
                var categoryDataSettings = await _settingService.LoadSettingAsync<ErpCategoryDataSettings>(storeScope);


                var mapping = ToKeyValueDictionary(productSettings);

                await LoadProductAttributeMappings();

                var attributeMapping = string.Empty;
                if (mapping.ContainsKey("ProductAttributes"))
                {
                    attributeMapping = mapping["ProductAttributes"];
                    mapping.Remove("ProductAttributes");
                }

                var erpProductDataModels = new List<ErpProductDataModel>();


                var fullUrl = apiUrlSettings.ProductApiUrl.TrimEnd();

                var endpoint = await PrepareUrl(
                    erpRequest, 
                    erpProductGetRequestSettings, 
                    fullUrl, 
                    erpProductGetRequestSettings.ProductSyncLimit, 
                    additionalFiltersSetting : erpProductGetRequestSettings.AdditionalFilters);

                //await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Information, ErpSyncLevel.Product, "TestLog: Product API Url : ", $"URL :{endpoint}");

                var response = await _d365HttpClient.GetAsync(endpoint, ErpSyncLevel.Product);
                responseContent = await response.Content.ReadAsStringAsync();

                var processor = new DataPathProcessor(responseContent);
                var dataModels = processor.MapToObjects<ErpProductDataModel>(mapping);
                int idx = 0;
                foreach (var item in dataModels)
                {
                    var categoryMapping = ToKeyValueDictionary(categoryDataSettings, idx++);
                    item.ProductCategories = processor.MapToObjects<ErpCategoryDataModel>(categoryMapping);
                }

                if (!string.IsNullOrWhiteSpace(attributeMapping))
                {
                    idx = 0;

                    foreach (var item in dataModels)
                    {
                        foreach (var map in _productAttributeMappings)
                        {
                            var mappingDict = new Dictionary<string, string>
                        {
                            {   map.Key,
                                InsertIndex(map.Value, idx)
                            }
                        };

                            var attribute = processor.MapToObject<ErpProductAttributeDataModel>(mappingDict, "Value");
                            attribute.Name = map.Key;

                            item.ProductAttributes.Add(attribute);
                        }
                        idx++;
                    }
                }

                if (dataModels != null && dataModels.Any())
                {
                    erpProductDataModels.AddRange(dataModels);
                    erpResponseData.Data = erpProductDataModels;
                    //await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Information, ErpSyncLevel.Product, "TestLog: Product API Response After Mapping : ", $"MappedValue :\n{JsonConvert.SerializeObject(dataModels, Formatting.Indented)}");
                    erpResponseData.ErpResponseModel = new ErpResponseModel
                    {
                        Next = (int.Parse(erpRequest.Start) + erpProductGetRequestSettings.ProductSyncLimit).ToString(),
                    };
                }
                else
                {
                    erpResponseData.Data = null;
                    erpResponseData.ErpResponseModel = new ErpResponseModel
                    {
                        Next = null,
                    };
                }

            }
            catch (Exception ex)
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
                erpResponseData.ErpResponseModel.ErrorFullMessage = !string.IsNullOrEmpty(responseContent) ? responseContent : ex.StackTrace;
            }

            return erpResponseData;
        }
        #endregion

        #region Accounts
        public async Task<ErpResponseData<IList<ErpAccountDataModel>>> GetCustomersFromErpAsync(ErpGetRequestModel erpRequest)
        {
            var erpResponseData = new ErpResponseData<IList<ErpAccountDataModel>>();
            var responseContent = string.Empty;

            try
            {
                if (erpRequest == null)
                {
                    erpResponseData.ErpResponseModel.IsError = false;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = "Request body content no data";
                    return erpResponseData;
                }

                await LoadDateTimeFormat();
                var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                var apiUrlSettings = await _settingService.LoadSettingAsync<D365IntegrationApiUrlSettings>(storeScope);

                if (!IsValidD365IntegrationApiUrlSettings(apiUrlSettings, apiUrlSettings.AccountApiUrl))
                {
                    erpResponseData.ErpResponseModel.IsError = true;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = "BCS Integration Settings is not configured.";
                    return erpResponseData;
                }

                var accountRequestSetting = await _settingService.LoadSettingAsync<ErpAccountGetRequestSettings>(storeScope);
                var accountSetting = await _settingService.LoadSettingAsync<ErpAccountSetting>(storeScope);

                var mapping = ToKeyValueDictionary(accountSetting);
                var erpAccountDataModels = new List<ErpAccountDataModel>();

                var fullUrl = apiUrlSettings.AccountApiUrl.TrimEnd();

                var endpoint = await PrepareUrl(
                    erpRequest, 
                    accountRequestSetting, 
                    fullUrl, 
                    accountRequestSetting.AccountSyncLimit, 
                    additionalFiltersSetting: accountRequestSetting.AdditionalFilters);

                //await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Information, ErpSyncLevel.Account, "TestLog: Account API Url : ", $"URL :{endpoint}");

                var response = await _d365HttpClient.GetAsync(endpoint, ErpSyncLevel.Account);
                responseContent = await response.Content.ReadAsStringAsync();

                var processor = new DataPathProcessor(responseContent);
                var dataModels = processor.MapToObjects<ErpAccountDataModel>(mapping);

                foreach (var item in dataModels)
                    item.CreditLimitUsed = item.CurrentBalance ?? 0;

                if (dataModels != null && dataModels.Any())
                {
                    erpAccountDataModels.AddRange(dataModels);

                    //await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Information, ErpSyncLevel.Account, "TestLog: Account API Response After Mapping : ", $"MappedValue :\n{JsonConvert.SerializeObject(dataModels, Formatting.Indented)}");

                    erpResponseData.Data = erpAccountDataModels;
                    erpResponseData.ErpResponseModel = new ErpResponseModel
                    {
                        Next = (int.Parse(erpRequest.Start) + accountRequestSetting.AccountSyncLimit).ToString(),
                    };
                }
                else
                {
                    erpResponseData.Data = null;
                    erpResponseData.ErpResponseModel = new ErpResponseModel
                    {
                        Next = null,
                    };
                }

            }
            catch (Exception ex)
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
                erpResponseData.ErpResponseModel.ErrorFullMessage = !string.IsNullOrEmpty(responseContent) ? responseContent : ex.StackTrace;
            }

            return erpResponseData;
        }

        public async Task<ErpResponseModel> CreateAccountNoErpAsync(ErpCreateAccountModel erpRequest)
        {
            var erpResponseData = new ErpResponseModel();
            var responseContent = string.Empty;

            try
            {
                if (erpRequest == null)
                {
                    erpResponseData.IsError = true;
                    erpResponseData.ErrorShortMessage = "Request body content has no data";
                    return erpResponseData;
                }
                await LoadDateTimeFormat();
                var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                var apiUrlSettings = await _settingService.LoadSettingAsync<D365IntegrationApiUrlSettings>(storeScope);

                if (!IsValidD365IntegrationApiUrlSettings(apiUrlSettings, apiUrlSettings.CreateAccountApiUrl))
                {
                    erpResponseData.IsError = false;
                    erpResponseData.ErrorShortMessage = "BCS Integration Settings is not configured.";
                    return erpResponseData;
                }

                var erpCreateAccountSettings = await _settingService.LoadSettingAsync<ErpCreateAccountSettings>(storeScope);

                var dict = await PrepareErpCreateRequestBodyAsync(erpRequest, erpCreateAccountSettings);

                await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Information, ErpSyncLevel.Account, "Payload for Creating Account : ", $"Payload :\n{JsonConvert.SerializeObject(dict, Formatting.Indented)}");


                var jsonPayload = JsonConvert.SerializeObject(dict);
                jsonPayload = jsonPayload.Replace("\"Operator\"", "\"operator\"");

                var httpReqContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                var response = await _d365HttpClient.PostAsync(apiUrlSettings.CreateAccountApiUrl, httpReqContent, ErpSyncLevel.Account);
                responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    erpResponseData.IsError = true;
                    erpResponseData.ErrorShortMessage = "Failed to retrieve data. Account was not created. Click to view the request payload and the response.";
                    await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Error, ErpSyncLevel.Account, erpResponseData.ErrorShortMessage, $"\nResponse: {responseContent}");

                    return erpResponseData;
                }

                var jsonResponse = JObject.Parse(responseContent);

                await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Information, ErpSyncLevel.Account, "Creating Account Response: ", $"\nResponse: {responseContent}");

                var accountNo = jsonResponse["accountNo"]?.ToString();


                if (!string.IsNullOrWhiteSpace(accountNo))
                {
                    erpResponseData.AccountNumber = accountNo;
                }
                else
                {
                    erpResponseData.IsError = false;
                    erpResponseData.ErrorShortMessage = "Response has no account number.";
                    return erpResponseData;
                }
            }
            catch (Exception ex)
            {
                erpResponseData.IsError = true;
                erpResponseData.ErrorShortMessage = ex.Message;
                erpResponseData.ErrorFullMessage = !string.IsNullOrEmpty(responseContent) ? responseContent : ex.StackTrace;
            }

            return erpResponseData;
        }

        #endregion

        #region Ship to address
        public async Task<ErpResponseData<IList<ErpShipToAddressDataModel>>> GetShipToAddressFromErpAsync(ErpGetRequestModel erpRequest)
        {
            var erpResponseData = new ErpResponseData<IList<ErpShipToAddressDataModel>>();
            var responseContent = string.Empty;

            try
            {
                if (erpRequest == null)
                {
                    erpResponseData.ErpResponseModel.IsError = false;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = "Request body content no data";
                    return erpResponseData;
                }

                await LoadDateTimeFormat();
                var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                var apiUrlSettings = await _settingService.LoadSettingAsync<D365IntegrationApiUrlSettings>(storeScope);

                if (!IsValidD365IntegrationApiUrlSettings(apiUrlSettings, apiUrlSettings.ShippingAddressApiUrl))
                {
                    erpResponseData.ErpResponseModel.IsError = true;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = "BCS Integration Settings is not configured.";
                    return erpResponseData;
                }

                var shipToAddressSettings = await _settingService.LoadSettingAsync<ErpShipToAddressSettings>(storeScope);
                var erpShipToAddressGetRequestSettings = await _settingService.LoadSettingAsync<ErpShipToAddressGetRequestSettings>(storeScope);

                var mapping = ToKeyValueDictionary(shipToAddressSettings);
                var erpShipToAddressDataModels = new List<ErpShipToAddressDataModel>();

                var fullUrl = apiUrlSettings.ShippingAddressApiUrl.TrimEnd();

                var endpoint = await PrepareUrl(
                    erpRequest, 
                    erpShipToAddressGetRequestSettings, 
                    fullUrl, 
                    erpShipToAddressGetRequestSettings.ShipToAddressSyncLimit, 
                    additionalFiltersSetting: erpShipToAddressGetRequestSettings.AdditionalFilters);

                //await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Information, ErpSyncLevel.ShipToAddress, "TestLog: ShipToAddress API Url : ", $"URL :{endpoint}");

                var response = await _d365HttpClient.GetAsync(endpoint, ErpSyncLevel.ShipToAddress);
                responseContent = await response.Content.ReadAsStringAsync();

                var processor = new DataPathProcessor(responseContent);
                var dataModels = processor.MapToObjects<ErpShipToAddressDataModel>(mapping);

                if (dataModels != null && dataModels.Any())
                {
                    erpShipToAddressDataModels.AddRange(dataModels);

                    //await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Information, ErpSyncLevel.ShipToAddress, "TestLog: ShipToAddress API Response After Mapping : ", $"MappedValue :\n{JsonConvert.SerializeObject(dataModels, Formatting.Indented)}");

                    erpResponseData.Data = erpShipToAddressDataModels;
                    erpResponseData.ErpResponseModel = new ErpResponseModel
                    {
                        Next = (int.Parse(erpRequest.Start) + erpShipToAddressGetRequestSettings.ShipToAddressSyncLimit).ToString(),
                    };
                }
                else
                {
                    erpResponseData.Data = null;
                    erpResponseData.ErpResponseModel = new ErpResponseModel
                    {
                        Next = null,
                    };
                }

            }
            catch (Exception ex)
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
                erpResponseData.ErpResponseModel.ErrorFullMessage = !string.IsNullOrEmpty(responseContent) ? responseContent : ex.StackTrace;
            }

            return erpResponseData;
        }

        public async Task<ErpResponseData<ErpShipToAddressDataModel>> CreateShipToAddressOnErpAsync(ErpShipToAddressCreateModel erpRequest)
        {
            var erpResponseData = new ErpResponseData<ErpShipToAddressDataModel>();
            var responseContent = string.Empty;

            try
            {
                if (erpRequest == null)
                {
                    erpResponseData.ErpResponseModel.IsError = true;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = "Request body content has no data";
                    return erpResponseData;
                }
                await LoadDateTimeFormat();
                var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                var apiUrlSettings = await _settingService.LoadSettingAsync<D365IntegrationApiUrlSettings>(storeScope);

                if (!IsValidD365IntegrationApiUrlSettings(apiUrlSettings, apiUrlSettings.CreateAccountApiUrl))
                {
                    erpResponseData.ErpResponseModel.IsError = true;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = "BCS Integration Settings is not configured.";
                    return erpResponseData;
                }

                var erpCreateShipToAddressSettings = await _settingService.LoadSettingAsync<ErpCreateShipToAddressSettings>(storeScope);

                var dict = await PrepareErpCreateRequestBodyAsync(erpRequest, erpCreateShipToAddressSettings);

                await _erpLogsService.InsertErpLogAsync(
                    ErpLogLevel.Information,
                    ErpSyncLevel.ShipToAddress,
                    "Payload for Creating ShipToAddress : ",
                    $"Payload :\n{JsonConvert.SerializeObject(dict, Formatting.Indented)}"
                );

                var jsonPayload = JsonConvert.SerializeObject(dict);
                jsonPayload = jsonPayload.Replace("\"Operator\"", "\"operator\"");

                var httpReqContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                var response = await _d365HttpClient.PostAsync(apiUrlSettings.CreateShipToAddressApiUrl, httpReqContent, ErpSyncLevel.ShipToAddress);
                responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    erpResponseData.ErpResponseModel.IsError = true;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = "Failed to retrieve data. ShipToAddress was not created. Click to view the request payload and the response.";
                    await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Error, ErpSyncLevel.ShipToAddress, erpResponseData.ErpResponseModel.ErrorShortMessage, $"\nResponse: {responseContent}");

                    return erpResponseData;
                }

                var originalData = JsonConvert.DeserializeObject<object>(responseContent);
                var transformedData = new { value = new[] { originalData } };
                var transformedJson = JsonConvert.SerializeObject(transformedData, Formatting.Indented);

                var shipToAddressSettings = await _settingService.LoadSettingAsync<ErpShipToAddressSettings>(storeScope);
                var mapping = ToKeyValueDictionary(shipToAddressSettings);

                var processor = new DataPathProcessor(transformedJson);
                var dataModels = processor.MapToObjects<ErpShipToAddressDataModel>(mapping);

                var jsonResponse = JObject.Parse(responseContent);

                await _erpLogsService.InsertErpLogAsync(
                    ErpLogLevel.Information,
                    ErpSyncLevel.ShipToAddress,
                    "ShipToAddress API Response and Mapping: ",
                    $"\nResponse: {responseContent}\n\nMappedValue:\n{JsonConvert.SerializeObject(dataModels, Formatting.Indented)}"
                );


                if (dataModels != null && dataModels.Any())
                {
                    erpResponseData.Data = dataModels.FirstOrDefault();
                    erpResponseData.ErpResponseModel = new ErpResponseModel
                    {
                        IsError = false
                    };
                }
                else
                {
                    erpResponseData.Data = null;
                    erpResponseData.ErpResponseModel = new ErpResponseModel
                    {
                        IsError = true,
                        ErrorShortMessage = "Account has no ShipToAddress."
                    };
                }
            }
            catch (Exception ex)
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
                erpResponseData.ErpResponseModel.ErrorFullMessage = !string.IsNullOrEmpty(responseContent) ? responseContent : ex.StackTrace;
            }

            return erpResponseData;
        }
        #endregion

        #region Invoice
        public async Task<ErpResponseData<IList<ErpInvoiceDataModel>>> GetInvoiceByAccountNoFromErpAsync(ErpGetRequestModel erpRequest)
        {
            var erpResponseData = new ErpResponseData<IList<ErpInvoiceDataModel>>();
            var responseContent = string.Empty;

            try
            {
                if (erpRequest == null)
                {
                    erpResponseData.ErpResponseModel.IsError = false;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = "Request body content no data";
                    return erpResponseData;
                }

                await LoadDateTimeFormat();
                var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                var apiUrlSettings = await _settingService.LoadSettingAsync<D365IntegrationApiUrlSettings>(storeScope);

                if (!IsValidD365IntegrationApiUrlSettings(apiUrlSettings, apiUrlSettings.InvoiceApiUrl))
                {
                    erpResponseData.ErpResponseModel.IsError = true;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = "BCS Integration Settings is not configured.";
                    return erpResponseData;
                }

                var invoiceDataSettings = await _settingService.LoadSettingAsync<ErpInvoiceDataSettings>(storeScope);
                var erpInvoiceGetRequestSettings = await _settingService.LoadSettingAsync<ErpInvoiceGetRequestSettings>(storeScope);

                var mapping = ToKeyValueDictionary(invoiceDataSettings);
                var validErpInvoiceDataModels = new List<ErpInvoiceDataModel>();

                var fullUrl = apiUrlSettings.InvoiceApiUrl.TrimEnd();

                var endpoint = await PrepareUrl(erpRequest, erpInvoiceGetRequestSettings, fullUrl, erpInvoiceGetRequestSettings.InvoiceSyncLimit);

                var response = await _d365HttpClient.GetAsync(endpoint, ErpSyncLevel.Invoice);
                responseContent = await response.Content.ReadAsStringAsync();

                var processor = new DataPathProcessor(responseContent);
                var dataModels = processor.MapToObjects<ErpInvoiceDataModel>(mapping);

                await LoadAdditionalMappings();

                if (!mapping.Equals(default(Dictionary<string, string>)))
                {
                    foreach (var invoice in dataModels)
                    {
                        //map document type, keep if it has proper document type

                        if (string.IsNullOrEmpty(invoice.DocumentType))
                            continue;

                        var additionalMappings = _additionalMappings
                                                    .FirstOrDefault(kvp => kvp.Key == invoice.DocumentType);

                        if (additionalMappings.Equals(default(KeyValuePair<string, string>)))
                            continue;

                        invoice.DocumentType = additionalMappings.Value;
                        validErpInvoiceDataModels.Add(invoice);
                    }
                }

                if (dataModels != null && dataModels.Any())
                {
                    erpResponseData.Data = validErpInvoiceDataModels;
                    erpResponseData.ErpResponseModel = new ErpResponseModel
                    {
                        Next = (int.Parse(erpRequest.Start) + erpInvoiceGetRequestSettings.InvoiceSyncLimit).ToString(),
                    };
                }
                else
                {
                    erpResponseData.Data = null;
                    erpResponseData.ErpResponseModel = new ErpResponseModel
                    {
                        Next = null,
                    };
                }

            }
            catch (Exception ex)
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
                erpResponseData.ErpResponseModel.ErrorFullMessage = !string.IsNullOrEmpty(responseContent) ? responseContent : ex.StackTrace;
            }

            return erpResponseData;
        }

        public async Task<ErpResponseData<string>> GetInvoicePdfByteCodeByDocumentNoFromErpAsync(ErpGetRequestModel erpRequest)
        {
            var erpResponseData = new ErpResponseData<string>();
            var responseContent = string.Empty;

            try
            {

                if (erpRequest == null)
                {
                    erpResponseData.ErpResponseModel.IsError = false;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = "Request body content no data";
                    return erpResponseData;
                }

                var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                var apiUrlSettings = await _settingService.LoadSettingAsync<D365IntegrationApiUrlSettings>(storeScope);

                if (!IsValidD365IntegrationApiUrlSettings(apiUrlSettings, apiUrlSettings.InvoicePdfApiUrl))
                {
                    erpResponseData.ErpResponseModel.IsError = true;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = "BCS Integration Settings is not configured.";
                    return erpResponseData;
                }

                var erpInvoicePdfSetting = await _settingService.LoadSettingAsync<ErpInvoicePdfSettings>(storeScope);
                var erpInvoicePdfGetRequestSettings = await _settingService.LoadSettingAsync<ErpInvoicePdfGetRequestSettings>(storeScope);

                var mapping = ToKeyValueDictionary(erpInvoicePdfSetting);

                var payloadData = PrepareRequestBody(erpRequest, erpInvoicePdfGetRequestSettings);

                //make httpcall

                var jsonPayload = JsonConvert.SerializeObject(payloadData);
                jsonPayload = jsonPayload.Replace("\"Operator\"", "\"operator\"");

                var httpReqContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");


                var response = await _d365HttpClient.PostAsync(apiUrlSettings.InvoicePdfApiUrl, httpReqContent, ErpSyncLevel.Invoice);
                responseContent = await response.Content.ReadAsStringAsync();


                var processor = new DataPathProcessor(responseContent);
                var dataModels = processor.MapToObjects<ErpInvoicePdfResponseModel>(mapping);

                if (dataModels != null && dataModels.Any())
                {
                    var pdfDataModel = dataModels.FirstOrDefault();
                    erpResponseData.Data = pdfDataModel?.InvoicePdfData ?? string.Empty;
                    erpResponseData.ErpResponseModel = new ErpResponseModel
                    {
                        Next = (int.Parse(erpRequest.Start) + 1).ToString(),
                    };
                }
                else
                {
                    erpResponseData.Data = null;
                    erpResponseData.ErpResponseModel = new ErpResponseModel
                    {
                        Next = null,
                    };
                }

            }
            catch (Exception ex)
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
                erpResponseData.ErpResponseModel.ErrorFullMessage = !string.IsNullOrEmpty(responseContent) ? responseContent : ex.StackTrace;
            }

            return erpResponseData;

        }
        #endregion

        #region Order
        public async Task<ErpResponseData<IList<ErpPlaceOrderDataModel>>> GetOrdersByAccountFromErpAsync(ErpGetRequestModel erpRequest)
        {
            var erpResponseData = new ErpResponseData<IList<ErpPlaceOrderDataModel>>();
            var responseContent = string.Empty;

            try
            {
                if (erpRequest == null)
                {
                    erpResponseData.ErpResponseModel.IsError = false;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = "Request body content no data";
                    return erpResponseData;
                }
                await LoadDateTimeFormat();
                var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                var apiUrlSettings = await _settingService.LoadSettingAsync<D365IntegrationApiUrlSettings>(storeScope);

                if (!IsValidD365IntegrationApiUrlSettings(apiUrlSettings, apiUrlSettings.OrderApiUrl))
                {
                    erpResponseData.ErpResponseModel.IsError = true;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = "BCS Integration Settings is not configured.";
                    return erpResponseData;
                }

                await LoadAdditionalMappings();

                var orderSettings = await _settingService.LoadSettingAsync<ErpOrderSettings>(storeScope);
                var orderShippingAddressSettings = await _settingService.LoadSettingAsync<ErpOrderShippingAddressSettings>(storeScope);
                var orderBillingAddressSettings = await _settingService.LoadSettingAsync<ErpPlaceOrderBillingAddressSettings>(storeScope);
                var erpOrderGetRequestSettings = await _settingService.LoadSettingAsync<ErpOrderGetRequestSettings>(storeScope);
                var orderItemDataSettings = await _settingService.LoadSettingAsync<ErpOrderItemDataSettings>(storeScope);
                var configSettings = await _settingService.LoadSettingAsync<D365IntegrationSettings>(storeScope);

                var mapping = ToKeyValueDictionary(orderSettings);
                var shippingAddressMapping = ToKeyValueDictionary(orderShippingAddressSettings, "ShippingAddress");
                var billingAddressMapping = ToKeyValueDictionary(orderBillingAddressSettings, "BillingAddress");

                foreach (var item in shippingAddressMapping)
                    mapping.Add(item.Key, item.Value);

                foreach (var item in billingAddressMapping)
                    mapping.Add(item.Key, item.Value);

                var erpOrderDataModels = new List<ErpPlaceOrderDataModel>();


                var fullUrl = apiUrlSettings.OrderApiUrl.TrimEnd();

                var endpoint = await PrepareUrl(erpRequest, erpOrderGetRequestSettings, fullUrl, erpOrderGetRequestSettings.OrderSyncLimit);

                var response = await _d365HttpClient.GetAsync(endpoint, ErpSyncLevel.Order);
                responseContent = await response.Content.ReadAsStringAsync();


                var processor = new DataPathProcessor(responseContent);
                var dataModels = processor.MapToObjects<ErpPlaceOrderDataModel>(mapping);
                int idx = 0;
                foreach (var order in dataModels)
                {
                    var orderItemMapping = ToKeyValueDictionary(orderItemDataSettings, idx++);
                    order.ErpPlaceOrderItemDatas = processor.MapToObjects<ErpPlaceOrderItemDataModel>(orderItemMapping);

                    // Map DeliveryMethod
                    if (!string.IsNullOrEmpty(order.DeliveryMethod))
                    {
                        var deliveryMapping = _additionalMappings
                            .FirstOrDefault(kvp => kvp.Key == order.DeliveryMethod);

                        if (!deliveryMapping.Equals(default(KeyValuePair<string, string>)))
                            order.DeliveryMethod = deliveryMapping.Value;
                    }

                    // Map OrderType
                    if (!string.IsNullOrEmpty(order.OrderType))
                    {
                        var orderTypeMapping = _additionalMappings
                            .FirstOrDefault(kvp => kvp.Key == order.OrderType);

                        if (!orderTypeMapping.Equals(default(KeyValuePair<string, string>)))
                            order.OrderType = orderTypeMapping.Value;
                    }

                }

                if (dataModels != null && dataModels.Any())
                {
                    erpOrderDataModels.AddRange(dataModels);
                    erpResponseData.Data = erpOrderDataModels;
                    erpResponseData.ErpResponseModel = new ErpResponseModel
                    {
                        Next = (int.Parse(erpRequest.Start) + erpOrderGetRequestSettings.OrderSyncLimit).ToString(),
                    };
                }
                else
                {
                    erpResponseData.Data = null;
                    erpResponseData.ErpResponseModel = new ErpResponseModel
                    {
                        Next = null,
                    };
                }

            }
            catch (Exception ex)
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
                erpResponseData.ErpResponseModel.ErrorFullMessage = !string.IsNullOrEmpty(responseContent) ? responseContent : ex.StackTrace;
            }

            return erpResponseData;

        }

        public async Task<ErpResponseData<Dictionary<int, string>>> CreateOrderAsync(ErpPlaceOrderDataModel erpRequest, List<int> orderItemIds = null)
        {
            var erpResponseData = new ErpResponseData<Dictionary<int, string>>();
            var responseContent = string.Empty;
            try
            {
                if (erpRequest == null)
                {
                    erpResponseData.ErpResponseModel.IsError = false;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = "Request body content no data";
                    return erpResponseData;
                }

                await LoadDateTimeFormat();
                await LoadAdditionalMappings();

                LimitCharacters(erpRequest);

                var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                var apiUrlSettings = await _settingService.LoadSettingAsync<D365IntegrationApiUrlSettings>(storeScope);

                bool isHeaderUrlValid = IsValidD365IntegrationApiUrlSettings(apiUrlSettings, apiUrlSettings.PlaceOrderHeaderApiUrl);
                bool isLineUrlValid = !apiUrlSettings.IsOrderItemExcluded ||
                                      IsValidD365IntegrationApiUrlSettings(apiUrlSettings, apiUrlSettings.PlaceOrderLinesApiUrl);

                if (!isHeaderUrlValid || !isLineUrlValid)
                {
                    erpResponseData.ErpResponseModel.IsError = false;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = "BCS Integration Settings is not configured.";
                    return erpResponseData;
                }

                var erpPlaceOrderSettings = await _settingService.LoadSettingAsync<ErpPlaceOrderSettings>(storeScope);
                var erpPlaceOrderShippingAddressSettings = await _settingService.LoadSettingAsync<ErpPlaceOrderShippingAddressSettings>(storeScope);
                var erpPlaceOrderBillingAddressSettings = await _settingService.LoadSettingAsync<ErpPlaceOrderBillingAddressSettings>(storeScope);
                var erpPlaceOrderItemSettings = await _settingService.LoadSettingAsync<ErpPlaceOrderItemSettings>(storeScope);
                var response = new HttpResponseMessage();
                var httpReqContent = new StringContent(string.Empty);
                var jsonResponse = new JObject();
                var statusDictionary = new Dictionary<int, string>();

                if (!apiUrlSettings.IsOrderItemExcluded)
                {
                    var payload = await PreparePlaceOrderBodyAsync(erpRequest, erpPlaceOrderSettings, erpPlaceOrderItemSettings, erpPlaceOrderShippingAddressSettings, erpPlaceOrderBillingAddressSettings);

                    await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Information, ErpSyncLevel.Order, "Payload for Creating Order: ", $"Payload :\n{JsonConvert.SerializeObject(payload, Formatting.Indented)}");

                    var serialized = JsonConvert.SerializeObject(payload);
                    serialized = serialized.Replace("\"Operator\"", "\"operator\"");

                    httpReqContent = new StringContent(serialized, Encoding.UTF8, "application/json");

                    response = await _d365HttpClient.PostAsync(apiUrlSettings.PlaceOrderHeaderApiUrl, httpReqContent, ErpSyncLevel.Order);
                    responseContent = await response.Content.ReadAsStringAsync();

                    jsonResponse = JObject.Parse(responseContent);

                    await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Information, ErpSyncLevel.Order, "TestLog: Creating Order Response: ", $"Response :\n{JsonConvert.SerializeObject(jsonResponse, Formatting.Indented)}");

                    var orderNumber = jsonResponse["orderNumber"]?.ToString();

                    if (string.IsNullOrWhiteSpace(orderNumber))
                    {
                        erpResponseData.ErpResponseModel.IsError = false;
                        erpResponseData.ErpResponseModel.ErrorShortMessage = "Response has no order number";
                        return erpResponseData;
                    }

                    if (orderItemIds != null)
                    {
                        foreach (var id in orderItemIds)
                            statusDictionary.Add(id, "success");
                    }
                    erpResponseData.Data = statusDictionary;

                    if (response.IsSuccessStatusCode)
                    {
                        erpResponseData.ErpResponseModel.OrderNumber = orderNumber;
                        return erpResponseData;
                    }

                }


                var dict = await PrepareErpOrderRequestBodyAsync(erpRequest, erpPlaceOrderSettings);

                if (erpRequest.ShippingAddress != null)
                {
                    var shippingDict = await PrepareErpOrderRequestBodyAsync(erpRequest.ShippingAddress, erpPlaceOrderShippingAddressSettings);
                    dict = MergeDictionaries(dict, shippingDict);
                }

                if (erpRequest.BillingAddress != null)
                {
                    var billingDict = await PrepareErpOrderRequestBodyAsync(erpRequest.BillingAddress, erpPlaceOrderBillingAddressSettings);
                    dict = MergeDictionaries(dict, billingDict);
                }

                #region replace values with transtation table

                var literals = new[] { "DELIVERY", "COLLECT", nameof(ErpOrderType.B2BSalesOrder), nameof(ErpOrderType.B2CSalesOrder), nameof(ErpOrderType.B2BQuote), nameof(ErpOrderType.B2BQuote) };


                string findKeyByValueFromModel(string value)
                {
                    return dict
                        .FirstOrDefault(kvp => string.Equals(kvp.Value?.ToString(), value, StringComparison.OrdinalIgnoreCase))
                        .Key ?? string.Empty;
                }

                foreach (var literal in literals)
                {

                    var key = findKeyByValueFromModel(literal);
                    if (dict.ContainsKey(key))
                    {
                        var toReplace = FindKeyByValue(literal);
                        if (!string.IsNullOrWhiteSpace(toReplace))
                        {
                            dict[key] = toReplace;
                        }
                    }
                }
                #endregion

                await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Information, ErpSyncLevel.Order, "Payload for Creating Order Header: ", $"Payload :\n{JsonConvert.SerializeObject(dict, Formatting.Indented)}");


                var jsonPayload = JsonConvert.SerializeObject(dict);
                jsonPayload = jsonPayload.Replace("\"Operator\"", "\"operator\"");

                httpReqContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                response = await _d365HttpClient.PostAsync(apiUrlSettings.PlaceOrderHeaderApiUrl, httpReqContent, ErpSyncLevel.Order);
                responseContent = await response.Content.ReadAsStringAsync();

                jsonResponse = JObject.Parse(responseContent);

                await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Information, ErpSyncLevel.Order, "Creating Order Header Response: ", $"Response :\n{JsonConvert.SerializeObject(jsonResponse, Formatting.Indented)}");

                var documentType = jsonResponse["Document_Type"]?.ToString();
                var orderNo = jsonResponse["No"]?.ToString();

                if (string.IsNullOrWhiteSpace(documentType) || string.IsNullOrWhiteSpace(orderNo))
                {
                    erpResponseData.ErpResponseModel.IsError = false;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = "Response has no order number or type";
                    return erpResponseData;
                }

                if (erpRequest.ErpPlaceOrderItemDatas == null)
                {
                    erpResponseData.ErpResponseModel.IsError = false;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = "Request has no items";
                    return erpResponseData;
                }


                var orderLinesBody = await PrepareErpOrderItemRequestBodyAsync(erpRequest, erpPlaceOrderItemSettings, orderNo, documentType, orderItemIds);

                jsonPayload = JsonConvert.SerializeObject(orderLinesBody);
                jsonPayload = jsonPayload.Replace("\"Operator\"", "\"operator\"");

                httpReqContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Information, ErpSyncLevel.Order, "TestLog: Payload for Creating Order Items: ", $"Payload :\n{JsonConvert.SerializeObject(orderLinesBody, Formatting.Indented)}");


                response = await _d365HttpClient.PostAsync(apiUrlSettings.PlaceOrderLinesApiUrl, httpReqContent, ErpSyncLevel.Order);
                responseContent = await response.Content.ReadAsStringAsync();

                jsonResponse = JObject.Parse(responseContent);

                await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Information, ErpSyncLevel.Order, "TestLog: Creating Order Items Response: ", $"Response :\n{JsonConvert.SerializeObject(jsonResponse, Formatting.Indented)}");

                foreach (var item in jsonResponse["responses"])
                {
                    var id = int.Parse(item["id"].ToString());
                    var status = int.Parse(item["status"].ToString());

                    var statusMessage = status == 201 ? "success" : "failed";

                    if (status != 201)
                    {
                        var errorMessage = item["body"]?["error"]?["message"]?.ToString() ?? "Unknown error";

                        await _erpLogsService.InformationAsync(
                            $"Order Line Failed:\nID: {id}\nStatus: {status}\nError Message: {errorMessage}",
                            ErpSyncLevel.Order
                        );
                    }

                    statusDictionary.Add(id, statusMessage);
                }

                if (statusDictionary.Count() != erpRequest.ErpPlaceOrderItemDatas.Count())
                {
                    for (int id = statusDictionary.Count() + 1; id <= erpRequest.ErpPlaceOrderItemDatas.Count(); id++)
                    {
                        statusDictionary.Add(id, "failed");
                    }
                }
                erpResponseData.Data = statusDictionary;
             

                if (response.IsSuccessStatusCode)
                {
                    erpResponseData.ErpResponseModel.OrderNumber = orderNo;
                    return erpResponseData;
                }

            }
            catch (Exception ex)
            {
                erpResponseData.Data = new Dictionary<int, string>();
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
                erpResponseData.ErpResponseModel.ErrorFullMessage = !string.IsNullOrEmpty(responseContent) ? responseContent : ex.StackTrace;
            }

            return erpResponseData;
        }
        #endregion

        #region Stocks
        public async Task<ErpResponseData<IList<ErpStockDataModel>>> GetStocksFromErpAsync(ErpGetRequestModel erpRequest)
        {
            var erpResponseData = new ErpResponseData<IList<ErpStockDataModel>>();
            var responseContent = string.Empty;

            try
            {
                if (erpRequest == null)
                {
                    erpResponseData.ErpResponseModel.IsError = false;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = "Request body content no data";
                    return erpResponseData;
                }

                await LoadDateTimeFormat();

                var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                var apiUrlSettings = await _settingService.LoadSettingAsync<D365IntegrationApiUrlSettings>(storeScope);

                if (!IsValidD365IntegrationApiUrlSettings(apiUrlSettings, apiUrlSettings.StockApiUrl))
                {
                    erpResponseData.ErpResponseModel.IsError = true;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = "BCS Integration Settings is not configured.";
                    return erpResponseData;
                }

                var stockRequestSetting = await _settingService.LoadSettingAsync<ErpStockGetRequestSettings>(storeScope);
                var stockSetting = await _settingService.LoadSettingAsync<ErpStockSettings>(storeScope);

                var mapping = ToKeyValueDictionary(stockSetting);
                var erpStockDataModels = new List<ErpStockDataModel>();

                //await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Information, ErpSyncLevel.Stock, "TestLog: Payload: ", $"Payload :\n{JsonConvert.SerializeObject(erpRequest, Formatting.Indented)}");

                var fullUrl = apiUrlSettings.StockApiUrl.TrimEnd();

                var endpoint = await PrepareUrl(erpRequest, stockRequestSetting, fullUrl, stockRequestSetting.StockSyncLimit);

                //await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Information, ErpSyncLevel.Stock, "TestLog: Stock API Url : ", $"URL :{endpoint}");

                var response = await _d365HttpClient.GetAsync(endpoint, ErpSyncLevel.Stock);
                responseContent = await response.Content.ReadAsStringAsync();

                //await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Information, ErpSyncLevel.Stock, "TestLog: Stock API Response : ", $"Response :\n{JsonConvert.SerializeObject(responseContent, Formatting.Indented)}");

                var processor = new DataPathProcessor(responseContent);

                var dataModels = processor.MapToObjects<ErpStockDataModel>(mapping);

                foreach (var item in dataModels)
                {
                    if (item.QuantityOnHand != null && item.QuantityOnSalesOrder != null && item.QuantityOnSalesOrder > 0)
                    {
                        item.QuantityOnHand -= item.QuantityOnSalesOrder;
                    }
                }

                //await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Information, ErpSyncLevel.Stock, "TestLog: Stock API Response After Mapping : ", $"MappedValue :\n{JsonConvert.SerializeObject(dataModels, Formatting.Indented)}");


                if (dataModels != null && dataModels.Any())
                {
                    erpStockDataModels.AddRange(dataModels);
                    erpResponseData.Data = erpStockDataModels;
                    erpResponseData.ErpResponseModel = new ErpResponseModel
                    {
                        Next = (int.Parse(erpRequest.Start) + stockRequestSetting.StockSyncLimit).ToString(),
                    };
                }
                else
                {
                    erpResponseData.Data = null;
                    erpResponseData.ErpResponseModel = new ErpResponseModel
                    {
                        Next = null,
                    };
                }

            }
            catch (Exception ex)
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
                erpResponseData.ErpResponseModel.ErrorFullMessage = !string.IsNullOrEmpty(responseContent) ? responseContent : ex.StackTrace;
            }

            return erpResponseData;

        }

        #endregion

        #region Pricing
        public async Task<ErpResponseData<IList<ErpPriceSpecialPricingDataModel>>> GetPerAccountProductPricingFromErpAsync(ErpGetRequestModel erpRequest)
        {
            var erpResponseData = new ErpResponseData<IList<ErpPriceSpecialPricingDataModel>>();
            var responseContent = string.Empty;

            try
            {
                if (erpRequest == null)
                {
                    erpResponseData.ErpResponseModel.IsError = false;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = "Request body content no data";
                    return erpResponseData;
                }

                await LoadDateTimeFormat();
                var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                var apiUrlSettings = await _settingService.LoadSettingAsync<D365IntegrationApiUrlSettings>(storeScope);

                if (!IsValidD365IntegrationApiUrlSettings(apiUrlSettings, apiUrlSettings.SpecialPriceApiUrl))
                {
                    erpResponseData.ErpResponseModel.IsError = true;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = "BCS Integration Settings is not configured.";
                    return erpResponseData;
                }

                var priceSpecialPricingDataSettings = await _settingService.LoadSettingAsync<ErpPriceSpecialPricingDataSettings>(storeScope);
                var erpSpecialPriceGetRequestSettings = await _settingService.LoadSettingAsync<ErpSpecialPriceGetRequestSettings>(storeScope);

                var mapping = ToKeyValueDictionary(priceSpecialPricingDataSettings);
                var erpSpecialPriceDataModels = new List<ErpPriceSpecialPricingDataModel>();

                var fullUrl = apiUrlSettings.SpecialPriceApiUrl.TrimEnd();

                var endpoint = await PrepareUrl(erpRequest, erpSpecialPriceGetRequestSettings, fullUrl, erpSpecialPriceGetRequestSettings.SpecialPriceSyncLimit);


                var response = await _d365HttpClient.GetAsync(endpoint, ErpSyncLevel.SpecialPrice);
                responseContent = await response.Content.ReadAsStringAsync();

                var processor = new DataPathProcessor(responseContent);
                var dataModels = processor.MapToObjects<ErpPriceSpecialPricingDataModel>(mapping);

                if (dataModels != null && dataModels.Any())
                {
                    erpSpecialPriceDataModels.AddRange(dataModels);
                    erpResponseData.Data = erpSpecialPriceDataModels;
                    erpResponseData.ErpResponseModel = new ErpResponseModel
                    {
                        Next = (int.Parse(erpRequest.Start) + erpSpecialPriceGetRequestSettings.SpecialPriceSyncLimit).ToString(),
                    };
                }
                else
                {
                    erpResponseData.Data = null;
                    erpResponseData.ErpResponseModel = new ErpResponseModel
                    {
                        Next = null,
                    };
                }

            }
            catch (Exception ex)
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
                erpResponseData.ErpResponseModel.ErrorFullMessage = !string.IsNullOrEmpty(responseContent) ? responseContent : ex.StackTrace;
            }

            return erpResponseData;
        }
        public async Task<ErpResponseData<IList<ErpPriceGroupPricingDataModel>>> GetProductGroupPricingFromErpAsync(ErpGetRequestModel erpRequest)
        {
            var erpResponseData = new ErpResponseData<IList<ErpPriceGroupPricingDataModel>>();
            var responseContent = string.Empty;

            try
            {
                if (erpRequest == null)
                {
                    erpResponseData.ErpResponseModel.IsError = false;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = "Request body content no data";
                    return erpResponseData;
                }

                await LoadDateTimeFormat();
                var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                var apiUrlSettings = await _settingService.LoadSettingAsync<D365IntegrationApiUrlSettings>(storeScope);

                if (!IsValidD365IntegrationApiUrlSettings(apiUrlSettings, apiUrlSettings.GroupPriceApiUrl))
                {
                    erpResponseData.ErpResponseModel.IsError = true;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = "BCS Integration Settings is not configured.";
                    return erpResponseData;
                }

                var groupPriceRequestSettings = await _settingService.LoadSettingAsync<ErpGroupPriceGetRequestSettings>(storeScope);
                var groupPriceSettings = await _settingService.LoadSettingAsync<ErpPriceGroupPricingDataSettings>(storeScope);

                var mapping = ToKeyValueDictionary(groupPriceSettings);
                var groupPriceMapping = string.Empty;

                if (mapping.ContainsKey("GroupPrices"))
                {
                    groupPriceMapping = mapping["GroupPrices"];
                    mapping.Remove("GroupPrices");
                }

                var erpGroupPriceDataModels = new List<ErpPriceGroupPricingDataModel>();

                var fullUrl = apiUrlSettings.GroupPriceApiUrl.TrimEnd();

                var endpoint = await PrepareUrl(erpRequest, groupPriceRequestSettings, fullUrl, groupPriceRequestSettings.GroupPriceSyncLimit);

                //await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Information, ErpSyncLevel.GroupPrice, "TestLog: Group Price API Url : ", $"URL :{endpoint}");

                var response = await _d365HttpClient.GetAsync(endpoint, ErpSyncLevel.GroupPrice);
                responseContent = await response.Content.ReadAsStringAsync();

                var processor = new DataPathProcessor(responseContent);
                var dataModels = processor.MapToObjects<ErpPriceGroupPricingDataModel>(mapping);

                if (!string.IsNullOrWhiteSpace(groupPriceMapping))
                {
                    var idx = 0;

                    var groupPriceMappings = ExtractKeyValuePairs(groupPriceMapping);

                    foreach (var item in dataModels)
                    {
                        foreach (var map in groupPriceMappings)
                        {
                            var mappingDict = new Dictionary<string, string>
                        {
                            {   map.Key,
                                InsertIndex(map.Value, idx)
                            }
                        };

                            var attribute = processor.MapToObject<ErpGroupPriceListDataModel>(mappingDict, "Value");
                            attribute.Name = map.Key;

                            item.GroupPrices.Add(attribute);
                        }
                        idx++;
                    }
                }

                if (dataModels != null && dataModels.Any())
                {
                    erpGroupPriceDataModels.AddRange(dataModels);

                    //await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Information, ErpSyncLevel.GroupPrice, "TestLog: Group Price API Response After Mapping : ", $"MappedValue :\n{JsonConvert.SerializeObject(dataModels, Formatting.Indented)}");

                    erpResponseData.Data = erpGroupPriceDataModels;
                    erpResponseData.ErpResponseModel = new ErpResponseModel
                    {
                        Next = (int.Parse(erpRequest.Start) + groupPriceRequestSettings.GroupPriceSyncLimit).ToString(),
                    };
                }
                else
                {
                    erpResponseData.Data = null;
                    erpResponseData.ErpResponseModel = new ErpResponseModel
                    {
                        Next = null,
                    };
                }

            }
            catch (Exception ex)
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
                erpResponseData.ErpResponseModel.ErrorFullMessage = !string.IsNullOrEmpty(responseContent) ? responseContent : ex.StackTrace;
            }

            return erpResponseData;
        }
        #endregion
        public (bool IsValid, string ErrorMessage) ValidateAdditionalHardCodedValuesSettings(string jsonSetting, bool checkType = true)
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

                // Value Validation
                if (setting.Value == null)
                    return (false, $"Value is required for key '{setting.Key}'.");

                if (checkType)
                {
                    if (string.IsNullOrEmpty(setting.Type))
                        return (false, $"In hard-coded values, type is required for key '{setting.Key}'");

                    setting.Type = setting.Type.Trim();

                    if (!_validTypes.Contains(setting.Type.ToLowerInvariant()))
                        return (false, $"In hard-coded values, invalid type found for key '{setting.Key}'. Valid types: string, int, bool, decimal, datetime");
                }
            }

            return (true, string.Empty);
        }

        public (bool IsValid, string ErrorMessage) ValidateTranslationTableModelSettings(string jsonString)
        {
            if (string.IsNullOrEmpty(jsonString))
                return (true, string.Empty);

            TranslationTableModel? mappings;
            try
            {
                mappings = JsonConvert.DeserializeObject<TranslationTableModel>(jsonString);
            }
            catch (JsonException)
            {
                return (false, "Invalid JSON format for translation table. Please provide values in proper JSON format.");
            }

            if (mappings == null)
                return (false, "No data found to process.");

            if (mappings.OrderTypes != null && mappings.OrderTypes.Any())
            {
                foreach (var item in mappings.OrderTypes)
                {
                    if (item is null)
                        return (false, "Invalid JSON format for OrderTypes. Please provide values in a proper json format.");


                    if (string.IsNullOrWhiteSpace(item.Key))
                        return (false, "Each order types value must have a non-empty 'key' that is not whitespace.");

                    if (item.Value == null)
                        return (false, $"Value is required for key '{item.Key}'.");
                }
            }

            if (mappings.DeliveryMethods != null && mappings.DeliveryMethods.Any())
            {
                foreach (var item in mappings.DeliveryMethods)
                {
                    if (item is null)
                        return (false, "Invalid JSON format for DeliveryMethods. Please provide values in a proper json format.");

                    if (string.IsNullOrWhiteSpace(item.Key))
                        return (false, "Each delivery method value must have a non-empty 'key' that is not whitespace.");

                    if (item.Value == null)
                        return (false, $"Value is required for key '{item.Key}'.");
                }
            }

            if (mappings.DocumentTypes != null && mappings.DocumentTypes.Any())
            {
                foreach (var item in mappings.DocumentTypes)
                {
                    if (item is null)
                        return (false, "Invalid JSON format for DocumentTypes. Please provide values in a proper json format.");

                    if (string.IsNullOrWhiteSpace(item.Key))
                        return (false, "Each document type value must have a non-empty 'key' that is not whitespace.");

                    if (item.Value == null)
                        return (false, $"Value is required for key '{item.Key}'.");
                }
            }
            return (true, string.Empty);
        }

        public (bool IsValid, string ErrorMessage) ValidateProductAttributeMappings(string jsonString)
        {
            if (string.IsNullOrEmpty(jsonString))
                return (true, string.Empty);

            List<ErpProductAttributeModel>? mappings;
            try
            {
                mappings = JsonConvert.DeserializeObject<List<ErpProductAttributeModel>>(jsonString);
            }
            catch (JsonException)
            {
                return (false, "Invalid JSON format for product attribute mappings. Please provide values in proper JSON format.");
            }

            if (mappings == null || !mappings.Any())
                return (false, "No product attribute mappings found to process.");

            foreach (var item in mappings)
            {
                if (item is null)
                    return (false, "One or more entries in the product attribute mappings are null or improperly formatted.");

                if (string.IsNullOrWhiteSpace(item.Key))
                    return (false, "Each product attribute must have a non-empty 'Key' that is not whitespace.");

                if (item.Value == null)
                    return (false, $"Value is required for Key '{item.Key}'.");
            }

            return (true, string.Empty);
        }


        #endregion
    }
}