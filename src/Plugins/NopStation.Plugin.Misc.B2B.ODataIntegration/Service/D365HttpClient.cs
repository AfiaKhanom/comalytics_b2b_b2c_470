using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nop.Core;
using Nop.Services.Configuration;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.Misc.B2B.ODataIntegration.Settings;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.Services
{
    public class D365HttpClient
    {
        #region Fields

        private readonly HttpClient _httpClient;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IErpLogsService _erpLogsService;
        private readonly D365IntegrationSettings _settings;
        private static string _accessToken;
        private static DateTime _tokenExpiryTime;

        #endregion

        #region Ctor

        public D365HttpClient(
            HttpClient httpClient,
            ISettingService settingService,
            IStoreContext storeContext,
            IErpLogsService erpLogsService,
            D365IntegrationSettings settings)
        {
            _httpClient = httpClient;
            _settingService = settingService;
            _storeContext = storeContext;
            _erpLogsService = erpLogsService;
            _settings = settings;
            _httpClient.Timeout = TimeSpan.FromSeconds(settings.ErpCallTimeOut > 0 ? settings.ErpCallTimeOut : D365IntegrationDefaults.DefaultTimeOutPeriod);
        }

        #endregion

        #region Utilities

        private async Task<string> GetAccessTokenAsync()
        {
            if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiryTime)
                return _accessToken;

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var apiUrlSettings = await _settingService.LoadSettingAsync<D365IntegrationApiUrlSettings>(storeScope);

            var tokenRequest = new Dictionary<string, string>
            {
                ["grant_type"] = D365IntegrationDefaults.GrantType,
                ["client_id"] = apiUrlSettings.ClientId,
                ["client_secret"] = apiUrlSettings.ClientSecret,
                ["scope"] = apiUrlSettings.Scope,
            };

            var tokenResponse = await _httpClient.PostAsync(apiUrlSettings.AccessTokenUrl,
                new FormUrlEncodedContent(tokenRequest));

            if (!tokenResponse.IsSuccessStatusCode)
                throw new Exception($"Failed to obtain access token. Status: {tokenResponse.StatusCode}");

            var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
            var tokenData = JsonConvert.DeserializeObject<Dictionary<string, string>>(tokenContent);

            _accessToken = tokenData["access_token"];
            _tokenExpiryTime = DateTime.UtcNow.AddSeconds(int.Parse(tokenData["expires_in"]));


            return _accessToken;
        }

        #endregion

        #region Methods
        public async Task<HttpResponseMessage> SendAsync(string requestUri, ErpSyncLevel syncLevel, HttpContent content = null)
        {
            var currentRetries = 0;
            HttpResponseMessage response = null;
            var request = new HttpRequestMessage();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<D365IntegrationSettings>(storeScope);

            while (currentRetries <= settings.HttpCallMaxRetries)
            {
                try
                {
                    // Get fresh token and add to request
                    var token = await GetAccessTokenAsync();

                    if(content != null)
                    {
                        request = new HttpRequestMessage(HttpMethod.Post, requestUri)
                        {
                            Content = content
                        };
                    }
                    else
                    {
                        request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                    }
                    request.Headers.Authorization = new AuthenticationHeaderValue(D365IntegrationDefaults.TokenType, token);

                    response = await _httpClient.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                        break;

                    var jsonResponse = JObject.Parse(await response.Content.ReadAsStringAsync());

                    await _erpLogsService.InsertErpLogAsync(
                        ErpLogLevel.Information, 
                        syncLevel,
                        $"HTTP Response status is unsuccessful. " +
                        ((settings.HttpCallMaxRetries - currentRetries > 0) ?
                        $"HTTP call will be retried after {settings.HttpCallRestTimeInMinutes} minutes. Retry attempts left: {settings.HttpCallMaxRetries - currentRetries}" :
                        "No retry attempts left.Click view to see the error."),
                        $"Error :\n{JsonConvert.SerializeObject(jsonResponse, Formatting.Indented)}");

                    if (settings.HttpCallMaxRetries - currentRetries <= 0)
                        response.EnsureSuccessStatusCode();

                    await Task.Delay(settings.HttpCallRestTimeInMinutes * 60 * 1000);
                }
                catch (Exception ex)
                {
                    await _erpLogsService.ErrorAsync(ex.Message, syncLevel);
                    if (currentRetries >= settings.HttpCallMaxRetries)
                        throw;
                }

                currentRetries++;
            }

            return response;
        }

        public async Task<HttpResponseMessage> GetAsync(string requestUri, ErpSyncLevel syncLevel)
        {
            return await SendAsync(requestUri, syncLevel);
        }

        public async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content, ErpSyncLevel syncLevel)
        {
            return await SendAsync(requestUri, syncLevel, content);
        }
        #endregion
    }
}