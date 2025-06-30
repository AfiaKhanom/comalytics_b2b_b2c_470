using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.Misc.ErpSqlIntegration.Models;

namespace NopStation.Plugin.Misc.ErpSqlIntegration.Services;
public class B2BAccountService : IB2BAccountService
{
    #region Fields

    private readonly ISqlIntegrationService _sqlIntegrationService;
    private readonly SqlClient _sqlClient;
    private readonly IErpNopMapperService _erpNopMapperService;

    #endregion

    #region Ctor

    public B2BAccountService(ISqlIntegrationService sqlIntegrationService,
        SqlClient sqlClient,
        IErpNopMapperService erpNopMapperService)
    {
        _sqlIntegrationService = sqlIntegrationService;
        _sqlClient = sqlClient;
        _erpNopMapperService = erpNopMapperService;
    }

    #endregion

    #region Methods

    public async Task<ErpResponseModel> CreateAccountNoErpAsync(ErpCreateAccountModel erpCreateAccountModel)
    {
        throw new NotImplementedException();
    }

    public async Task<ErpResponseData<IList<ErpAccountDataModel>>> GetAccountsFromErpAsync(ErpGetRequestModel erpRequest)
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

            if (!await _sqlIntegrationService.IsValidSqlIntegrationSettings())
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = "Sql Integration Settings is not configured well.";
                return erpResponseData;
            }

            var response = await _sqlClient.ProcessSqlQueryWithRequestModelAsync(ErpSyncLevel.Account, erpRequest);

            if (!response.Success || response.Data is null)
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = $"Executing SQL failed due to - {response.Message}";
                return erpResponseData;
            }

            var erpAccountsResponseData = JsonConvert.DeserializeObject<IList<ErpAccountSqlResponseModel>>(response.Data.ToString());

            if (erpAccountsResponseData != null && erpAccountsResponseData.Any())
            {
                erpResponseData.Data = await _erpNopMapperService.ErpAccountMapNop(erpAccountsResponseData);
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

    public async Task<ErpResponseData<IList<ErpInvoiceDataModel>>> GetInvoiceByAccountNoFromErpAsync(ErpGetRequestModel erpRequest)
    {
        //var erpResponseData = new ErpResponseData<IList<ErpInvoiceDataModel>>();
        //var responseContent = string.Empty;

        //try
        //{
        //    #region First get the document numbers

        //    var dateFrom = erpRequest.DateFrom.HasValue ? erpRequest.DateFrom.Value.ToString("yyyy-MM-dd") : DateTime.MinValue.ToString("yyyy-MM-dd");

        //    var sqlText = _sqlIntegrationService.ParameterizeQueryString("Select document From Invoices Where AccNum = '@AccountNumber' AND (OrdDate >= '@DateFrom' OR ChangedDate >= '@DateFrom') Order By document",
        //        new
        //        {
        //            AccountNumber = erpRequest.AccountNumber,
        //            DateFrom = dateFrom
        //        });

        //    var serialized = new IQApiRequestGeneratorModel(_retailIntegrationSettings).CreateApiRequest(
        //                               requestType: IQRetailIntegrationDefaults.IQApiRequestGenericSQL,
        //                               filterType: "",
        //                               textFilter: "",
        //                               recordOffset: int.Parse(erpRequest.Start),
        //                               sqlText: sqlText).ToString();

        //    if (!_sqlIntegrationService.IsValidIQRetailIntegrationSettings(_retailIntegrationSettings))
        //    {
        //        erpResponseData.ErpResponseModel.IsError = true;
        //        erpResponseData.ErpResponseModel.ErrorShortMessage = "IQ Retail Integration Settings is not configured.";
        //        return erpResponseData;
        //    }

        //    var response = await _iQRetailHttpClient.HttpCall(IQRetailIntegrationDefaults.IQApiRequestGenericSQL, serialized, ErpSyncLavel.Invoice);
        //    if (!response.IsSuccessStatusCode)
        //    {
        //        erpResponseData.ErpResponseModel.IsError = true;
        //        erpResponseData.ErpResponseModel.ErrorShortMessage = response.ToString();
        //        return erpResponseData;
        //    }

        //    responseContent = await response.Content.ReadAsStringAsync();
        //    var rootRecordData = JsonConvert.DeserializeObject<IQApiResultInvoiceRecordModel>(responseContent);
        //    var erpInvByAccResponseData = rootRecordData.InvoiceRecordModel.ErpInvoiceRecords.ToList();

        //    if (rootRecordData is not null && rootRecordData.IQApiErrors[0].ErrorCode != 0 || !erpInvByAccResponseData.Any())
        //    {
        //        erpResponseData.ErpResponseModel.IsError = true;
        //        erpResponseData.ErpResponseModel.ErrorShortMessage = $"Invoice record in Erp for Erp Account ({erpRequest.AccountNumber}): {rootRecordData?.IQApiErrors?[0].ErrorDescription}";
        //        erpResponseData.ErpResponseModel.ErrorFullMessage = responseContent;
        //        return erpResponseData;
        //    }

        //    #endregion

        //    #region Then get the invoices

        //    var documents = "(" + string.Join(" , ", erpInvByAccResponseData.Select(invRecords => $"'{invRecords.Document}'")) + ")";

        //    var textFilter = _sqlIntegrationService.ParameterizeQueryString("document In @documents",
        //        new
        //        {
        //            documents = documents
        //        });

        //    serialized = new IQApiRequestGeneratorModel(_retailIntegrationSettings).CreateApiRequest(
        //                               requestType: IQRetailIntegrationDefaults.IQApiRequestDocumentInvoice,
        //                               filterType: IQRetailIntegrationDefaults.EApiFilterText,
        //                               textFilter: textFilter,
        //                               recordOffset: int.Parse(erpRequest.Start),
        //                               sqlText: "").ToString();

        //    response = await _iQRetailHttpClient.HttpCall(IQRetailIntegrationDefaults.IQApiRequestDocumentInvoice, serialized, ErpSyncLavel.Invoice);
        //    if (!response.IsSuccessStatusCode)
        //    {
        //        erpResponseData.ErpResponseModel.IsError = true;
        //        erpResponseData.ErpResponseModel.ErrorShortMessage = response.ToString();
        //        return erpResponseData;
        //    }

        //    responseContent = await response.Content.ReadAsStringAsync();
        //    var rootData = JsonConvert.DeserializeObject<IQApiResultInvoiceModel>(responseContent);
        //    if (rootData is not null && rootData.IQApiErrors[0].ErrorCode != 0)
        //    {
        //        erpResponseData.ErpResponseModel.IsError = true;
        //        erpResponseData.ErpResponseModel.ErrorShortMessage = $"Invoice record in Erp for Erp Account ({erpRequest.AccountNumber}): {rootRecordData?.IQApiErrors?[0].ErrorDescription}";
        //        erpResponseData.ErpResponseModel.ErrorFullMessage = responseContent;
        //        return erpResponseData;
        //    }

        //    var erpInvByDocsResponseData = rootData.IQApiResultDataModel.IQRootJson.ProcessingDocuments.ToList();
        //    erpResponseData.Data = await _erpNopMapperService.ErpInvoiceMapNop(erpInvByDocsResponseData);
        //    erpResponseData.ErpResponseModel = new ErpResponseModel
        //    {
        //        Next = rootData.IQApiPageData.NextOffset.ToString()
        //    };

        //    #endregion
        //}
        //catch (Exception ex)
        //{
        //    erpResponseData.ErpResponseModel.IsError = true;
        //    erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
        //    erpResponseData.ErpResponseModel.ErrorFullMessage = !string.IsNullOrEmpty(responseContent) ? responseContent : ex.StackTrace;
        //}

        //return erpResponseData;

        throw new NotImplementedException();
    }

    public async Task<ErpResponseData<string>> GetInvoicePdfByteCodeByDocumentNoFromErpAsync(ErpGetRequestModel erpRequest)
    {
        //var erpResponseData = new ErpResponseData<string>();
        //var responseContent = string.Empty;

        //try
        //{
        //    var textFilter = _sqlIntegrationService.ParameterizeQueryString("Document = '@DocumentNumber'",
        //        new
        //        {
        //            DocumentNumber = erpRequest.DocumentNumber,
        //        });

        //    var serialized = new IQApiRequestGeneratorModel(_retailIntegrationSettings).CreateApiRequest(
        //                               requestType: IQRetailIntegrationDefaults.IQApiRequestDocumentInvoice,
        //                               filterType: IQRetailIntegrationDefaults.EApiFilterText,
        //                               textFilter: textFilter,
        //                               recordOffset: int.Parse(erpRequest.Start),
        //                               embeddingType: IQRetailIntegrationDefaults.PdfEmbedding,
        //                               sqlText: "").ToString();

        //    if (!_sqlIntegrationService.IsValidIQRetailIntegrationSettings(_retailIntegrationSettings))
        //    {
        //        erpResponseData.ErpResponseModel.IsError = true;
        //        erpResponseData.ErpResponseModel.ErrorShortMessage = "IQ Retail Integration Settings is not configured.";
        //        return erpResponseData;
        //    }

        //    var response = await _iQRetailHttpClient.HttpCall(IQRetailIntegrationDefaults.IQApiRequestDocumentInvoice, serialized, ErpSyncLavel.Invoice);
        //    if (!response.IsSuccessStatusCode)
        //    {
        //        erpResponseData.ErpResponseModel.IsError = true;
        //        erpResponseData.ErpResponseModel.ErrorShortMessage = response.ToString();
        //        return erpResponseData;
        //    }

        //    responseContent = await response.Content.ReadAsStringAsync();
        //    var rootData = JsonConvert.DeserializeObject<IQApiResultInvoicePdfModel>(responseContent);
        //    if (rootData is not null && rootData.IQApiErrors[0].ErrorCode != 0)
        //    {
        //        erpResponseData.ErpResponseModel.IsError = true;
        //        erpResponseData.ErpResponseModel.ErrorShortMessage = $"Invoice record in Erp for Document Number ({erpRequest.DocumentNumber}): {rootData.IQApiErrors[0].ErrorDescription}";
        //        erpResponseData.ErpResponseModel.ErrorFullMessage = responseContent;
        //        return erpResponseData;
        //    }

        //    var erpInvPdfByDocNoData = rootData.InvoicePdfModel.IQRootJsonForInvoicePdf.ErpInvoicePdfByteCodes.ToList();
        //    erpResponseData.Data = erpInvPdfByDocNoData[0].Document;
        //    erpResponseData.ErpResponseModel = new ErpResponseModel
        //    {
        //        Next = rootData.IQApiPageData.NextOffset.ToString()
        //    };

        //}
        //catch (Exception ex)
        //{
        //    erpResponseData.ErpResponseModel.IsError = true;
        //    erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
        //    erpResponseData.ErpResponseModel.ErrorFullMessage = !string.IsNullOrEmpty(responseContent) ? responseContent : ex.StackTrace;
        //}

        //return erpResponseData;

        throw new NotImplementedException();
    }

    public async Task<ErpResponseData<IList<ErpShipToAddressDataModel>>> GetShipToAddressByAccountNumberFromErpAsync(ErpGetRequestModel erpRequest)
    {
        //var erpResponseData = new ErpResponseData<IList<ErpShipToAddressDataModel>>();
        //var responseContent = string.Empty;

        //try
        //{
        //    var textFilter = _sqlIntegrationService.ParameterizeQueryString("account like '%@AccountNumber%'",
        //        new
        //        {
        //            AccountNumber = erpRequest.AccountNumber
        //        });

        //    var serialized = new IQApiRequestGeneratorModel(_retailIntegrationSettings).CreateApiRequest(
        //                               requestType: IQRetailIntegrationDefaults.IQApiRequestDebtor,
        //                               filterType: IQRetailIntegrationDefaults.EApiFilterText,
        //                               textFilter: textFilter,
        //                               recordOffset: int.Parse(erpRequest.Start),
        //                               sqlText: "").ToString();

        //    if (!_sqlIntegrationService.IsValidIQRetailIntegrationSettings(_retailIntegrationSettings))
        //    {
        //        erpResponseData.ErpResponseModel.IsError = true;
        //        erpResponseData.ErpResponseModel.ErrorShortMessage = "IQ Retail Integration Settings is not configured.";

        //        return erpResponseData;
        //    }

        //    var response = await _iQRetailHttpClient.HttpCall(IQRetailIntegrationDefaults.IQApiRequestDebtor, serialized, ErpSyncLavel.ShipToAddress);
        //    if (!response.IsSuccessStatusCode)
        //    {
        //        erpResponseData.ErpResponseModel.IsError = true;
        //        erpResponseData.ErpResponseModel.ErrorShortMessage = response.ToString();

        //        return erpResponseData;
        //    }

        //    responseContent = await response.Content.ReadAsStringAsync();
        //    var rootData = JsonConvert.DeserializeObject<IQApiResultDebtorModel>(responseContent);
        //    if (rootData is not null && rootData.IQApiErrors[0].ErrorCode != 0)
        //    {
        //        erpResponseData.ErpResponseModel.IsError = true;
        //        erpResponseData.ErpResponseModel.ErrorShortMessage = $"Ship To Address record in Erp for Erp Account ({erpRequest.AccountNumber}): {rootData.IQApiErrors[0].ErrorDescription}";
        //        erpResponseData.ErpResponseModel.ErrorFullMessage = responseContent;
        //        return erpResponseData;
        //    }

        //    var erpShipAddressByAccResponseData = rootData.IQApiResultDataModel.IQRootJson.DebtorsMasters.ToList();
        //    erpResponseData.Data = await _erpNopMapperService.ErpShipToAddressMapNop(erpShipAddressByAccResponseData);
        //    erpResponseData.ErpResponseModel = new ErpResponseModel
        //    {
        //        Next = rootData.IQApiPageData.NextOffset.ToString()
        //    };
        //}
        //catch (Exception ex)
        //{
        //    erpResponseData.ErpResponseModel.IsError = true;
        //    erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
        //    erpResponseData.ErpResponseModel.ErrorFullMessage = !string.IsNullOrEmpty(responseContent) ? responseContent : ex.StackTrace;
        //}

        //return erpResponseData;

        throw new NotImplementedException();
    }

    public async Task<ErpResponseData<IList<ErpAccountDataModel>>> GetAllAccountCreditFromErpAsync(ErpGetRequestModel erpRequest)
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
            var serialized = await _sqlIntegrationService.PrepareErpAccountCreditsRequestBody(erpRequest);

            if (!await _sqlIntegrationService.IsValidSqlIntegrationSettings())
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = "Sql Integration Settings is not configured well.";
                return erpResponseData;
            }

            var response = await _sqlClient.HttpCall(serialized, ErpSyncLevel.Account);
            if (!response.IsSuccessStatusCode)
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = response.ToString();
                return erpResponseData;
            }

            responseContent = await response.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(responseContent);
            List<ErpAccountSqlResponseModel> erpAccountsResponseData = null;
            // Check if the 'Data' field exists and is not null
            if (jsonResponse["Data"] != null)
            {
                erpAccountsResponseData = jsonResponse["Data"].ToObject<List<ErpAccountSqlResponseModel>>();
            }
            else
            {
                // If 'Data' is missing, try deserializing directly
                erpAccountsResponseData = JsonConvert.DeserializeObject<List<ErpAccountSqlResponseModel>>(responseContent);
            }

            if (erpAccountsResponseData != null && erpAccountsResponseData.Any())
            {
                erpResponseData.Data = await _erpNopMapperService.ErpAccountMapNop(erpAccountsResponseData);
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
}
