using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.Misc.B2B.SysproIntegration.Models;

namespace NopStation.Plugin.Misc.B2B.SysproIntegration.Services;
public class B2BStockService : IB2BStockService
{
    private readonly ISysproIntegrationService _sysproIntegrationService;
    private readonly SysproClient _sysproClient;
    private readonly IErpNopMapperService _erpNopMapperService;

    public B2BStockService(ISysproIntegrationService sysproIntegrationService,
        SysproClient sysproClient,
        IErpNopMapperService erpNopMapperService)
    {
        _sysproIntegrationService = sysproIntegrationService;
        _sysproClient = sysproClient;
        _erpNopMapperService = erpNopMapperService;
    }
    public async Task<ErpResponseData<IList<ErpStockDataModel>>> GetStockFromErpAsync(ErpGetRequestModel erpRequest)
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
            var serialized = await _sysproIntegrationService.PrepareErpStockRequestBody(erpRequest);

            if (!await _sysproIntegrationService.IsValidSysproIntegrationSettings())
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = "Syspro Integration Settings is not configured well.";
                return erpResponseData;
            }

            var response = await _sysproClient.HttpCall(serialized, ErpSyncLevel.Stock);
            if (!response.IsSuccessStatusCode)
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = response.ToString();
                return erpResponseData;
            }

            responseContent = await response.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(responseContent);
            List<ErpStockSysproResponseModel> erpStockResponseData = null;
            // Check if the 'Data' field exists and is not null
            if (jsonResponse["Data"] != null)
            {
                erpStockResponseData = jsonResponse["Data"].ToObject<List<ErpStockSysproResponseModel>>();
            }
            else
            {
                // If 'Data' is missing, try deserializing directly
                erpStockResponseData = JsonConvert.DeserializeObject<List<ErpStockSysproResponseModel>>(responseContent);
            }

            if (erpStockResponseData != null && erpStockResponseData.Any())
            {
                erpResponseData.Data = await _erpNopMapperService.ErpStockMapNop(erpStockResponseData);
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
