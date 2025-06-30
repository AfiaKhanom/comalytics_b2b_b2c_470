using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.Misc.ErpSqlIntegration.Models;

namespace NopStation.Plugin.Misc.ErpSqlIntegration.Services;
public class ShipToAddressService : IShipToAddressService
{
    private readonly ISqlIntegrationService _sqlIntegrationService;
    private readonly SqlClient _sqlClient;
    private readonly IErpNopMapperService _erpNopMapperService;

    public ShipToAddressService(ISqlIntegrationService sqlIntegrationService,
        SqlClient sqlClient,
        IErpNopMapperService erpNopMapperService)
    {
        _sqlIntegrationService = sqlIntegrationService;
        _sqlClient = sqlClient;
        _erpNopMapperService = erpNopMapperService;
    }

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

            if (!await _sqlIntegrationService.IsValidSqlIntegrationSettings())
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = "Sql Integration Settings is not configured well.";
                return erpResponseData;
            }
            var response = await _sqlClient.ProcessSqlQueryWithRequestModelAsync(ErpSyncLevel.ShipToAddress, erpRequest);

            if (!response.Success || response.Data is null)
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = $"Executing SQL failed due to - {response.Message}";
                return erpResponseData;
            }

            var erpShipToAddressResponseData = JsonConvert.DeserializeObject<List<ErpShipToAddressSqlResponseModel>>(response.Data.ToString());
           
            if (erpShipToAddressResponseData != null && erpShipToAddressResponseData.Any())
            {
                erpResponseData.Data = await _erpNopMapperService.ErpShipToAddressMapNop(erpShipToAddressResponseData);
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
