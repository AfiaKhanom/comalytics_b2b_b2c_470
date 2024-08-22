using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.Misc.B2B.SysproIntegration.Models;

namespace NopStation.Plugin.Misc.B2B.SysproIntegration.Services;
public class B2BPricingService : IB2BPricingService
{
    private readonly ISysproIntegrationService _sysproIntegrationService;
    private readonly SysproClient _sysproClient;
    private readonly IErpNopMapperService _erpNopMapperService;

    public B2BPricingService(ISysproIntegrationService sysproIntegrationService,
        SysproClient sysproClient,
        IErpNopMapperService erpNopMapperService)
    {
        _sysproIntegrationService = sysproIntegrationService;
        _sysproClient = sysproClient;
        _erpNopMapperService = erpNopMapperService;
    }
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
            var serialized = await _sysproIntegrationService.PrepareErpPriceSpecialPricingsRequestBody(erpRequest);

            if (!await _sysproIntegrationService.IsValidSysproIntegrationSettings())
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = "Syspro Integration Settings is not configured well.";
                return erpResponseData;
            }

            var response = await _sysproClient.HttpCall(serialized, ErpSyncLavel.Account);
            if (!response.IsSuccessStatusCode)
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = response.ToString();
                return erpResponseData;
            }

            responseContent = await response.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(responseContent);
            List<ErpPriceSpecialPricingSysproResponseModel> ErpPriceSpecialPricingsResponseData = null;
            // Check if the 'Data' field exists and is not null
            if (jsonResponse["Data"] != null)
            {
                ErpPriceSpecialPricingsResponseData = jsonResponse["Data"].ToObject<List<ErpPriceSpecialPricingSysproResponseModel>>();
            }
            else
            {
                // If 'Data' is missing, try deserializing directly
                ErpPriceSpecialPricingsResponseData = JsonConvert.DeserializeObject<List<ErpPriceSpecialPricingSysproResponseModel>>(responseContent);
            }

            if (ErpPriceSpecialPricingsResponseData != null && ErpPriceSpecialPricingsResponseData.Any())
            {
                erpResponseData.Data = await _erpNopMapperService.ErpPriceSpecialPricingMapNop(ErpPriceSpecialPricingsResponseData);
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
