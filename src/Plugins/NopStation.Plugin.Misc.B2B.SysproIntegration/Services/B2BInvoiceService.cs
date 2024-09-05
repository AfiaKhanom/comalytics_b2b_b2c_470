using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.Misc.B2B.SysproIntegration.Models;

namespace NopStation.Plugin.Misc.B2B.SysproIntegration.Services;
public class B2BInvoiceService : IB2BInvoiceService
{
    private readonly ISysproIntegrationService _sysproIntegrationService;
    private readonly SysproClient _sysproClient;
    private readonly IErpNopMapperService _erpNopMapperService;

    public B2BInvoiceService(ISysproIntegrationService sysproIntegrationService,
        SysproClient sysproClient,
        IErpNopMapperService erpNopMapperService)
    {
        _sysproIntegrationService = sysproIntegrationService;
        _sysproClient = sysproClient;
        _erpNopMapperService = erpNopMapperService;
    }
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
            var serialized = await _sysproIntegrationService.PrepareErpInvoiceRequestBody(erpRequest);

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
            List<ErpInvoiceSysproResponseModel> ErpShipToAddressResponseData = null;
            // Check if the 'Data' field exists and is not null
            if (jsonResponse["Data"] != null)
            {
                ErpShipToAddressResponseData = jsonResponse["Data"].ToObject<List<ErpInvoiceSysproResponseModel>>();
            }
            else
            {
                // If 'Data' is missing, try deserializing directly
                ErpShipToAddressResponseData = JsonConvert.DeserializeObject<List<ErpInvoiceSysproResponseModel>>(responseContent);
            }

            if (ErpShipToAddressResponseData != null && ErpShipToAddressResponseData.Any())
            {
                erpResponseData.Data = await _erpNopMapperService.ErpInvoiceMapNop(ErpShipToAddressResponseData);
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

    public Task<ErpResponseData<string>> GetInvoicePdfByteCodeByDocumentNoFromErpAsync(ErpGetRequestModel erpRequest)
    {
        throw new NotImplementedException();
    }
}
