using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.Misc.ErpSqlIntegration.Models;

namespace NopStation.Plugin.Misc.ErpSqlIntegration.Services;
public class B2BProductService : IB2BProductService
{
    private readonly ISqlIntegrationService _sqlIntegrationService;
    private readonly SqlClient _sqlClient;
    private readonly IErpNopMapperService _erpNopMapperService;

    public B2BProductService(ISqlIntegrationService sqlIntegrationService,
        SqlClient sqlClient,
        IErpNopMapperService erpNopMapperService)
    {
        _sqlIntegrationService = sqlIntegrationService;
        _sqlClient = sqlClient;
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

            if (!await _sqlIntegrationService.IsValidSqlIntegrationSettings())
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = "Sql Integration Settings is not configured well.";
                return erpResponseData;
            }

            var response = await _sqlClient.ProcessSqlQueryWithRequestModelAsync(ErpSyncLevel.Product, erpRequest);

            if (response == null ||
                !response.Success ||
                response.Data is null)
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = $"Executing SQL failed due to - {response.Message}";
                return erpResponseData;
            }

            var erpProductResponseData = JsonConvert.DeserializeObject<List<ErpProductSqlResponseModel>>(response.Data.ToString());

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
