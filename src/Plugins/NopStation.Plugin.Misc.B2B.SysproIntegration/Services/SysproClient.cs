using System.Net.Http.Headers;
using System.Text;

namespace NopStation.Plugin.Misc.B2B.SysproIntegration.Services;
public class SysproClient
{
    private readonly SysproIntegrationSettings _sysproIntegrationSettings;
    private readonly HttpClient _httpClient;

    public SysproClient(SysproIntegrationSettings sysproIntegrationSettings,
        HttpClient httpClient)
    {
        httpClient.BaseAddress = new Uri(sysproIntegrationSettings.BaseUrl);
        httpClient.Timeout = TimeSpan.FromSeconds(sysproIntegrationSettings.ErpCallTimeOut > 0 ? sysproIntegrationSettings.ErpCallTimeOut : SysproIntegrationDefaults.DefaultTimeOutPeriod);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("BearerToken", sysproIntegrationSettings.Token);
        _sysproIntegrationSettings = sysproIntegrationSettings;
        _httpClient = httpClient;
    }

    #region Method

    public async Task<HttpResponseMessage> HttpCall(string payloadData)
    {
        try
        {
            var currentRetries = 0;

            var httpReqContent = new StringContent(payloadData, Encoding.UTF8, "test/xml");

            var httpResponse = new HttpResponseMessage();

            // The loop should run atleast once. Therefore, count started from 0.
            while (currentRetries <= _sysproIntegrationSettings.HttpCallMaxRetries)
            {
                httpResponse = await _httpClient.PostAsync(_httpClient.BaseAddress, httpReqContent);

                if (!httpResponse.IsSuccessStatusCode)
                {
                    //await _erpLogsService.InformationAsync(
                    //    message: $"HTTP Response status is unsuccessful. " +
                    //    ((_sysproIntegrationSettings.HttpCallMaxRetries - currentRetries > 0) ?
                    //    $"HTTP call will be retried after {_sysproIntegrationSettings.HttpCallRestTimeInMinutes} minutes. Retry attempts left: {_sysproIntegrationSettings.HttpCallMaxRetries - currentRetries}" :
                    //    "No retry attempts left."),
                    //    syncLavel: erpSyncLabel);

                    if (_sysproIntegrationSettings.HttpCallMaxRetries - currentRetries <= 0)
                        httpResponse.EnsureSuccessStatusCode();

                    await Task.Delay(_sysproIntegrationSettings.HttpCallRestTimeInMinutes * 60 * 1000); // Converting the minute time into milliseconds.
                }
                else
                    break;

                currentRetries++;
            }

            return httpResponse;
        }
        catch (AggregateException exception)
        {
            throw exception.InnerException;
        }
    }

    #endregion
}
