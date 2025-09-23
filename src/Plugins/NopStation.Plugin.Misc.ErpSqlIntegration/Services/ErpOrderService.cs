using Newtonsoft.Json;
using Nop.Core;
using Nop.Services.Configuration;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.Misc.ErpSqlIntegration.Models;
using NopStation.Plugin.Misc.ErpSqlIntegration.Services.SqLQueryTemplates;

namespace NopStation.Plugin.Misc.ErpSqlIntegration.Services;

public class ErpOrderService : IErpOrderService
{
    private readonly ISqlIntegrationService _sqlIntegrationService;
    private readonly SqlClient _sqlClient;
    private readonly IErpNopMapperService _erpNopMapperService;
    private readonly IErpLogsService _erpLogsService;
    private readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;

    public ErpOrderService(ISqlIntegrationService sqlIntegrationService,
        SqlClient sqlClient,
        IErpNopMapperService erpNopMapperService,
        IErpLogsService erpLogsService,
        SqlIntegrationSettings sqlIntegrationSettings,
        ISettingService settingService,
        IStoreContext storeContext)
    {
        _sqlIntegrationService = sqlIntegrationService;
        _sqlClient = sqlClient;
        _erpNopMapperService = erpNopMapperService;
        _erpLogsService = erpLogsService;
        _settingService = settingService;
        _storeContext = storeContext;
    }

    private void ProcessOrderResponse(ErpResponseModel responseModel, ErpOrderResponse orderResponse)
    {
        if (responseModel == null || orderResponse == null)
        {
            responseModel.IsError = true;
            responseModel.ErrorShortMessage = "Response model or order response from ERP is null.";
            return;
        }

        if (orderResponse.HasError)
        {
            responseModel.IsError = true;
            responseModel.ErrorShortMessage = "Something went wrong, found error.";
            responseModel.ErrorFullMessage = orderResponse.ValidationErrors != null
                                                ? string.Join(" ", orderResponse.ValidationErrors)
                                                : string.Empty;
            return;
        }
        if (string.IsNullOrEmpty(orderResponse.ID))
        {
            responseModel.IsError = true;
            responseModel.ErrorShortMessage = "Something went wrong with response. In this response, the parameter-ID not found to extract order number.";
            responseModel.ErrorFullMessage = orderResponse.ValidationErrors != null
                                                ? string.Join(" ", orderResponse.ValidationErrors)
                                                : string.Empty;
            return;
        }

        // example - "ID": "Object:SalesOrderDto | ID:315700 | Reference:SO235501"
        var parts = orderResponse.ID.Split('|');
        foreach (var part in parts)
        {
            var keyValue = part.Split(new[] { ':' }, 2);
            if (keyValue.Length == 2)
            {
                if (keyValue[0].Trim().Equals("ID", StringComparison.OrdinalIgnoreCase))
                {
                    responseModel.OrderNumber = keyValue[1].Trim();
                    return;
                }
            }
        }

        // If we reach here, it means we didn't find the order number in the expected format
        responseModel.IsError = true;
        responseModel.ErrorShortMessage = "Something went wrong, no valid data in respones for getting order number.";
        responseModel.ErrorFullMessage = orderResponse.ValidationErrors != null
                                            ? string.Join(" ", orderResponse.ValidationErrors)
                                            : string.Empty;
        return;
    }

    public async Task<ErpResponseModel> CreateOrderOnErpAsync(ErpPlaceOrderDataModel erpRequest)
    {
        var erpResponseData = new ErpResponseModel();
        var responseContent = string.Empty;
        try
        {
            if (erpRequest == null)
            {
                erpResponseData.IsError = false;
                erpResponseData.ErrorShortMessage = "Request body content no data";
                return erpResponseData;
            }
            var settingsVadilation = await _sqlIntegrationService.IsValidSqlIntegrationSettingsForPlacingOrder();
            if (!settingsVadilation.IsValid)
            {
                erpResponseData.IsError = true;
                erpResponseData.ErrorShortMessage = $"SQL Integration Settings are not configured correctly. {settingsVadilation.ErrorMessage}";
                return erpResponseData;
            }

            var reqBody = await _sqlIntegrationService.PrepareErpOrderPlaceRequestBodyAsync(erpRequest);

            var sqlIntegrationSettings = await _settingService.LoadSettingAsync<SqlIntegrationSettings>(await _storeContext.GetActiveStoreScopeConfigurationAsync());

            var baseUrl = sqlIntegrationSettings.BaseUrl;
            if (!baseUrl.EndsWith("/"))
            {
                baseUrl += "/";
            }
            var fullUrl = new Uri(new Uri(baseUrl), "UVPVP/SDK/Rest/SalesOrderPlaceOrder").ToString();

            var response = await _sqlClient.HttpCall(reqBody, ErpSyncLevel.Order, fullUrl);
            if (!response.IsSuccessStatusCode)
            {
                erpResponseData.IsError = true;
                erpResponseData.ErrorShortMessage = "Something went wrong whlie placing order at ERP.";
                erpResponseData.ErrorFullMessage = response.ToString();
                return erpResponseData;
            }

            responseContent = await response.Content.ReadAsStringAsync();

            await _erpLogsService.InformationAsync($"Response after placing order at erp : {responseContent}", ErpSyncLevel.Order);

            var orderResponseFromErp = JsonConvert.DeserializeObject<ErpOrderResponse>(responseContent);

            ProcessOrderResponse(erpResponseData, orderResponseFromErp);
            return erpResponseData;
        }
        catch (Exception ex)
        {
            erpResponseData.IsError = true;
            erpResponseData.ErrorShortMessage = ex.Message;
            erpResponseData.ErrorFullMessage = !string.IsNullOrEmpty(responseContent) ? responseContent : ex.StackTrace;
        }

        return erpResponseData;
    }

    public async Task<ErpResponseData<IList<ErpPlaceOrderDataModel>>> GetOrderByAccountFromErpAsync(ErpGetRequestModel erpRequest)
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

            if (!await _sqlIntegrationService.IsValidSqlIntegrationSettings())
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = "SQL Integration Settings are not configured correctly.";
                return erpResponseData;
            }

            var response = await _sqlClient.ProcessSqlQueryWithRequestModelAsync(ErpSyncLevel.Order, erpRequest);

            if (response == null ||
                !response.Success ||
                response.Data is null)
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = $"Executing SQL failed due to - {response.Message}";
                return erpResponseData;
            }

            var erpOrdersResponseData = JsonConvert.DeserializeObject<List<ErpOrderSyncSqlResponseModel>>(response.Data.ToString());

            if (erpOrdersResponseData != null && erpOrdersResponseData.Any())
            {
                erpResponseData.Data = await _erpNopMapperService.ErpOrderMapNop(erpOrdersResponseData);
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
}