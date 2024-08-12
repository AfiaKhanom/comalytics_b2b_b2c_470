using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.Misc.B2B.SysproIntegration.Services;
public class SysproClient
{
    private readonly SysproIntegrationSettings _sysproIntegrationSettings;
    private readonly HttpClient _httpClient;
    private readonly IErpLogsService _erpLogsService;

    public SysproClient(SysproIntegrationSettings sysproIntegrationSettings,
        HttpClient httpClient,
        IErpLogsService erpLogsService)
    {
        _sysproIntegrationSettings = sysproIntegrationSettings;
        _httpClient = httpClient;
        _erpLogsService = erpLogsService;
    }

    #region Method

    public async Task<HttpResponseMessage> HttpCall(object payloadData, ErpSyncLavel erpSyncLevel)
    {
        try
        {
            var currentRetries = 0;
            HttpResponseMessage httpResponse = new HttpResponseMessage();

            if (string.IsNullOrWhiteSpace(_sysproIntegrationSettings.BaseUrl))
            {
                httpResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
                httpResponse.Content = new StringContent("BaseUrl cannot be null or empty.");
                return httpResponse;
            }

            // Prepare HttpClient properties
            _httpClient.BaseAddress = new Uri(_sysproIntegrationSettings.BaseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(_sysproIntegrationSettings.ErpCallTimeOut > 0 ? _sysproIntegrationSettings.ErpCallTimeOut : SysproIntegrationDefaults.DefaultTimeOutPeriod);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _sysproIntegrationSettings.Token);

            // Serialize the JSON object to a JSON string
            var jsonPayload = JsonConvert.SerializeObject(payloadData);

            // Log the payload
            await _erpLogsService.InformationAsync($"Serialized JSON Payload: {jsonPayload}", erpSyncLevel);

            var httpReqContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            // Retry loop
            while (currentRetries <= _sysproIntegrationSettings.HttpCallMaxRetries)
            {
                httpResponse = await _httpClient.PostAsync("", httpReqContent);

                if (!httpResponse.IsSuccessStatusCode)
                {
                    var errorMessage = await httpResponse.Content.ReadAsStringAsync();
                    await _erpLogsService.InformationAsync(
                        message: $"HTTP Response status is unsuccessful. " +
                        $"Response: {errorMessage}. " +
                        ((_sysproIntegrationSettings.HttpCallMaxRetries - currentRetries > 0) ?
                        $"HTTP call will be retried after {_sysproIntegrationSettings.HttpCallRestTimeInSeconds} seconds. Retry attempts left: {_sysproIntegrationSettings.HttpCallMaxRetries - currentRetries}" :
                        "No retry attempts left."),
                        syncLavel: erpSyncLevel);

                    if (currentRetries == _sysproIntegrationSettings.HttpCallMaxRetries)
                    {
                        // Construct an error response
                        var errorResponse = new HttpResponseMessage(httpResponse.StatusCode)
                        {
                            Content = new StringContent($"Failed after {currentRetries} retries. Last error message: {httpResponse.ReasonPhrase}. Response: {errorMessage}")
                        };
                        return errorResponse;
                    }

                    await Task.Delay(_sysproIntegrationSettings.HttpCallRestTimeInSeconds * 1000); // Converting the time into milliseconds.
                }
                else
                {
                    return httpResponse;
                }

                currentRetries++;
            }

            return httpResponse;
        }
        catch (Exception ex)
        {
            await _erpLogsService.ErrorAsync($"Exception occurred: {ex.Message}", erpSyncLevel);

            return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)
            {
                Content = new StringContent($"Exception occurred: {ex.Message}")
            };
        }
    }

    #endregion
}
