using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.Misc.ErpSqlIntegration.Models;

namespace NopStation.Plugin.Misc.ErpSqlIntegration.Services;
public class B2BInvoiceService : IB2BInvoiceService
{
    private readonly ISqlIntegrationService _sqlIntegrationService;
    private readonly SqlClient _sqlClient;
    private readonly IErpNopMapperService _erpNopMapperService;

    public B2BInvoiceService(ISqlIntegrationService sqlIntegrationService,
        SqlClient sqlClient,
        IErpNopMapperService erpNopMapperService)
    {
        _sqlIntegrationService = sqlIntegrationService;
        _sqlClient = sqlClient;
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

            if (!await _sqlIntegrationService.IsValidSqlIntegrationSettings())
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = "Sql Integration Settings is not configured well.";
                return erpResponseData;
            }

            var response = await _sqlClient.ProcessSqlQueryWithRequestModelAsync(ErpSyncLevel.Invoice, erpRequest);

            if (!response.Success || response.Data is null)
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = $"Executing SQL failed due to - {response.Message}";
                return erpResponseData;
            }

            var erpInvoiceResponseData = JsonConvert.DeserializeObject<List<ErpInvoiceSqlResponseModel>>(response.Data.ToString());

            if (erpInvoiceResponseData != null && erpInvoiceResponseData.Any())
            {
                erpResponseData.Data = await _erpNopMapperService.ErpInvoiceMapNop(erpInvoiceResponseData);
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
            var serialized = await _sqlIntegrationService.PrepareErpInvoiceHexRequestBody(erpRequest);

            if (!await _sqlIntegrationService.IsValidSqlIntegrationSettings())
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = "Sql Integration Settings is not configured well.";
                return erpResponseData;
            }

            var response = await _sqlClient.HttpCall(serialized, ErpSyncLevel.Invoice);
            if (!response.IsSuccessStatusCode)
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = response.ToString();
                return erpResponseData;
            }

            responseContent = await response.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(responseContent);

            if (jsonResponse["Success"] != null && jsonResponse["Success"].ToString().Equals("false"))
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = "Sql returned with error";
                return erpResponseData;
            }

            // Check if the 'Data' field exists and is not null
            if (jsonResponse["Data"] != null)
            {
                try
                {
                    System.Xml.XmlDocument xOut = new System.Xml.XmlDocument();
                    xOut.LoadXml(jsonResponse["Data"].ToString());
                    erpResponseData.Data = xOut.GetElementsByTagName("DocumentHex")[0].InnerText;
                }
                catch (Exception ex)
                {
                    erpResponseData.ErpResponseModel.IsError = false;
                    erpResponseData.Data = response.Data.ToString();
                    return erpResponseData;
                }
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

    public async Task<ErpResponseData<string>> GetStatementPdfByteCodeFromErpAsync(ErpGetRequestModel erpRequest)
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
            var serialized = await _sqlIntegrationService.PrepareErpStatementHexRequestBody(erpRequest);

            if (!await _sqlIntegrationService.IsValidSqlIntegrationSettings())
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = "Sql Integration Settings is not configured well.";
                return erpResponseData;
            }

            var response = await _sqlClient.HttpCall(serialized, ErpSyncLevel.Invoice);
            if (!response.IsSuccessStatusCode)
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = response.ToString();
                return erpResponseData;
            }

            responseContent = await response.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(responseContent);

            if (jsonResponse["Success"] != null && jsonResponse["Success"].ToString().Equals("false"))
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = "Sql returned with error";
                return erpResponseData;
            }

            // Check if the 'Data' field exists and is not null
            if (jsonResponse["Data"] != null)
            {
                try
                {
                    System.Xml.XmlDocument xOut = new System.Xml.XmlDocument();
                    xOut.LoadXml(jsonResponse["Data"].ToString());
                    erpResponseData.Data = xOut.GetElementsByTagName("DocumentHex")[0].InnerText;
                }
                catch (Exception ex)
                {
                    erpResponseData.ErpResponseModel.IsError = false;
                    erpResponseData.Data = response.Data.ToString();
                    return erpResponseData;
                }
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
