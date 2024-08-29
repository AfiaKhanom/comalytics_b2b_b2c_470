using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.Misc.B2B.SysproIntegration.Models;

namespace NopStation.Plugin.Misc.B2B.SysproIntegration.Services;
public class B2BProductService : IB2BProductService
{
    private readonly ISysproIntegrationService _sysproIntegrationService;
    private readonly SysproClient _sysproClient;
    private readonly IErpNopMapperService _erpNopMapperService;

    public B2BProductService(ISysproIntegrationService sysproIntegrationService,
        SysproClient sysproClient,
        IErpNopMapperService erpNopMapperService)
    {
        _sysproIntegrationService = sysproIntegrationService;
        _sysproClient = sysproClient;
        _erpNopMapperService = erpNopMapperService;
    }

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
            var serialized = await _sysproIntegrationService.PrepareErpProductRequestBody(erpRequest);

            if (!await _sysproIntegrationService.IsValidSysproIntegrationSettings())
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = "Syspro Integration Settings is not configured well.";
                return erpResponseData;
            }

            var response = await _sysproClient.HttpCall(serialized, ErpSyncLevel.Product);
            if (!response.IsSuccessStatusCode)
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = response.ToString();
                return erpResponseData;
            }

            responseContent = await response.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(responseContent);
            List<ErpProductSysproResponseModel> erpProductResponseData = null;

            // Check if the 'Data' field exists and is not null
            if (jsonResponse["Data"] != null)
            {
                erpProductResponseData = jsonResponse["Data"].ToObject<List<ErpProductSysproResponseModel>>();
            }
            else
            {
                // If 'Data' is missing, try deserializing directly
                erpProductResponseData = JsonConvert.DeserializeObject<List<ErpProductSysproResponseModel>>(responseContent);
            }

            if (erpProductResponseData != null && erpProductResponseData.Any())
            {
                erpResponseData.Data = await _erpNopMapperService.ErpProductMapNop(erpProductResponseData);
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
