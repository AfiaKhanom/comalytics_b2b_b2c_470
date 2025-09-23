using System.Data;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Data;
using Nop.Services.Configuration;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.Misc.ErpSqlIntegration.Models;
using NopStation.Plugin.Misc.ErpSqlIntegration.Services.SqLQueryTemplates;

namespace NopStation.Plugin.Misc.ErpSqlIntegration.Services
{
    public class SqlClient
    {
        private readonly HttpClient _httpClient;
        private readonly IErpLogsService _erpLogsService;
        private readonly ISqlQueryTemplatService _sqlQueryTemplatService;
        private readonly INopDataProvider _nopDataProvider;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;

        public SqlClient(
            HttpClient httpClient,
            IErpLogsService erpLogsService,
            ISqlQueryTemplatService sqlQueryTemplatService,
            INopDataProvider nopDataProvider,
            IWorkContext workContext,
            IStoreContext storeContext,
            ISettingService settingService)
        {
            _httpClient = httpClient;
            _erpLogsService = erpLogsService;
            _sqlQueryTemplatService = sqlQueryTemplatService;
            _nopDataProvider = nopDataProvider;
            _workContext = workContext;
            _storeContext = storeContext;
            _settingService = settingService;
        }

        #region Utilities

        private void ConfigureHttpClient(SqlIntegrationSettings sqlIntegrationSettings)
        {
            if (!string.IsNullOrWhiteSpace(sqlIntegrationSettings.BaseUrl))
            {
                _httpClient.BaseAddress = new Uri(sqlIntegrationSettings.BaseUrl);
            }
            _httpClient.Timeout = TimeSpan.FromMinutes(5);
            var byteArray = Encoding.ASCII.GetBytes($"{sqlIntegrationSettings.AuthUserName}:{sqlIntegrationSettings.AuthPassword}");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }

        private async Task<SqlResponseModel<object>> ExecuteSqlQueryAsync(SqlResponseModel<object> response, string sqlCommand, ErpSyncLevel erpSyncLevel, SqlIntegrationSettings sqlIntegrationSettings)
        {
            var currentRetries = 0;
            var maxRetries = sqlIntegrationSettings.HttpCallMaxRetries;
            var delayBetweenRetries = sqlIntegrationSettings.HttpCallRestTimeInSeconds * 1000;

            while (currentRetries < maxRetries)
            {
                try
                {
                    // fetch data using connection string
                    using (var connection = new SqlConnection(sqlIntegrationSettings.ConnectionString))
                    {
                        await connection.OpenAsync();

                        using (var command = new SqlCommand(sqlCommand, connection))
                        {
                            var dataTable = new DataTable();

                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                dataTable.Load(reader);
                            }
                            response.Data = (dataTable is null) ? "[]" : JsonConvert.SerializeObject(dataTable);
                            response.Success = true;
                            return response;
                        }
                    }
                }
                catch (Exception ex)
                {
                    await _erpLogsService.InformationAsync($"SQL Error: {ex.Message}. Retrying after {sqlIntegrationSettings.HttpCallRestTimeInSeconds} seconds.... Attempts left: {maxRetries - currentRetries - 1}", syncLevel: erpSyncLevel);

                    if (currentRetries == maxRetries - 1)
                    {
                        response.Message = $"Failed after {maxRetries} retries due to : {ex.Message}";
                        await _erpLogsService.ErrorAsync(response.Message, erpSyncLevel);
                        return response;
                    }
                }

                // retry after delay
                await Task.Delay(delayBetweenRetries);
                currentRetries++;
            }

            return response;
        }

        private async Task<string> GetParameterizeQueryString(string queryString, dynamic parameters)
        {
            if (!string.IsNullOrEmpty(queryString))
            {
                string startProperty = nameof(ErpGetRequestModel.Start); // in query, it should be an int value, not string

                foreach (var prop in parameters.GetType().GetProperties())
                {
                    var paramName = $"@{prop.Name}";
                    var paramValue = prop.GetValue(parameters);

                    // Skip if the parameter is not used in the query
                    if (!queryString.Contains(paramName))
                        continue;

                    switch (paramValue)
                    {
                        case null:
                            queryString = queryString.Replace(paramName, "NULL");
                            break;

                        case bool boolValue:
                            queryString = queryString.Replace(paramName, boolValue ? "1" : "0");
                            break;

                        case DateTime dateTimeValue:
                            // Format datetime in SQL Server compatible format (yyyy-MM-dd HH:mm:ss)
                            queryString = queryString.Replace(paramName, $"'{dateTimeValue:yyyy-MM-dd HH:mm:ss}'");
                            break;

                        case string stringValue:
                            queryString = queryString.Replace(paramName, prop.Name == startProperty ? stringValue : $"'{stringValue}'");
                            break;

                        default:
                            queryString = queryString.Replace(paramName, paramValue.ToString());
                            break;
                    }
                }
            }

            return await Task.FromResult(queryString);
        }

        #endregion Utilities

        #region Method

        public async Task<HttpResponseMessage> HttpCall(object payloadData, ErpSyncLevel erpSyncLevel, string path = "")
        {
            try
            {
                var currentRetries = 0;
                HttpResponseMessage httpResponse = new HttpResponseMessage();
                var sqlIntegrationSettings = await _settingService.LoadSettingAsync<SqlIntegrationSettings>(await _storeContext.GetActiveStoreScopeConfigurationAsync());
                if (sqlIntegrationSettings == null)
                    throw new Exception("SqlIntegrationSettings not found.");

                // Serialize the JSON object to a JSON string
                var jsonPayload = JsonConvert.SerializeObject(payloadData, Formatting.Indented);

                // Log the payload
                await _erpLogsService.InformationAsync($"Serialized JSON Payload: {jsonPayload}", erpSyncLevel);

                var httpReqContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                ConfigureHttpClient(sqlIntegrationSettings);

                // Retry loop
                while (currentRetries < sqlIntegrationSettings.HttpCallMaxRetries)
                {
                    httpResponse = await _httpClient.PostAsync(path, httpReqContent).ConfigureAwait(false);

                    if (!httpResponse.IsSuccessStatusCode)
                    {
                        var errorMessage = await httpResponse.Content.ReadAsStringAsync();
                        await _erpLogsService.InformationAsync(
                            message: $"HTTP Response status is unsuccessful. " +
                            $"Response: {errorMessage}. " +
                            ((sqlIntegrationSettings.HttpCallMaxRetries - currentRetries > 0) ?
                            $"HTTP call will be retried after {sqlIntegrationSettings.HttpCallRestTimeInSeconds} seconds. Retry attempts left: {sqlIntegrationSettings.HttpCallMaxRetries - currentRetries}" :
                            "No retry attempts left."),
                            syncLevel: erpSyncLevel);

                        if (currentRetries == sqlIntegrationSettings.HttpCallMaxRetries)
                        {
                            // Construct an error response
                            var errorResponse = new HttpResponseMessage(httpResponse.StatusCode)
                            {
                                Content = new StringContent($"Failed after {currentRetries} retries. Last error message: {httpResponse.ReasonPhrase}. Response: {errorMessage}")
                            };
                            return errorResponse;
                        }
                        if (sqlIntegrationSettings.HttpCallMaxRetries > 1)
                            await Task.Delay(sqlIntegrationSettings.HttpCallRestTimeInSeconds * 1000); // Converting the time into milliseconds.
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
                await _erpLogsService.ErrorAsync($"Exception occurred: {ex.Message}", erpSyncLevel, ex);

                return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent($"Exception message: {ex.Message}. Stacktrace: {ex.StackTrace}")
                };
            }
        }

        public async Task<SqlResponseModel<object>> ProcessSqlQueryWithRequestModelAsync(ErpSyncLevel erpSyncLevel, ErpGetRequestModel requestModel)
        {
            var response = new SqlResponseModel<object>();

            if (requestModel == null)
            {
                await _erpLogsService.ErrorAsync("Error - executing sql query failed, req model cannot be null.", erpSyncLevel);
                response.Message = "Error - executing sql query failed";
                return response;
            }

            try
            {
                var sqlIntegrationSettings = await _settingService.LoadSettingAsync<SqlIntegrationSettings>(await _storeContext.GetActiveStoreScopeConfigurationAsync());
                if (string.IsNullOrEmpty(sqlIntegrationSettings.ConnectionString))
                {
                    response.Message = "Connection string not found to fetch data";
                    return response;
                }

                // get the query
                var sqlQueryTemplate = await _sqlQueryTemplatService.GetSqlQueryTemplateBySyncLevelIdAsync((int)erpSyncLevel);
                if (sqlQueryTemplate is null || string.IsNullOrEmpty(sqlQueryTemplate.Query))
                {
                    response.Message = "No SQL query found for the specified sync level.";
                    return response;
                }

                if (requestModel.Limit <= 0)
                    requestModel.Limit = 100;

                // generate SQL command with parameters
                var sqlCommand = await GetParameterizeQueryString(sqlQueryTemplate.Query, requestModel);
                if (string.IsNullOrEmpty(sqlCommand))
                {
                    response.Message = "SQL command generation failed. Please verify request parameters.";
                    return response;
                }

                await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Information, erpSyncLevel, "Executing SQL query. View query here.", $"SQL query - {sqlCommand}", await _workContext.GetCurrentCustomerAsync());

                return await ExecuteSqlQueryAsync(response,
                    sqlCommand,
                    erpSyncLevel,
                    sqlIntegrationSettings);
            }
            catch (Exception ex)
            {
                await _erpLogsService.ErrorAsync($"Exception occurred: {ex.Message}", erpSyncLevel, ex);
                response.Message = $"Exception message: {ex.Message}. Stacktrace: {ex.StackTrace}";
                return response;
            }
        }

        #endregion Method
    }
}