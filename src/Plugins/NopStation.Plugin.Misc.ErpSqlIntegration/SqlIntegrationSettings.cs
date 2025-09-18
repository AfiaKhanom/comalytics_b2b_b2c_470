using Nop.Core.Configuration;

namespace NopStation.Plugin.Misc.ErpSqlIntegration;

public class SqlIntegrationSettings : ISettings
{
    public int ErpCallTimeOut { get; set; }
    public int HttpCallMaxRetries { get; set; }
    public int HttpCallRestTimeInSeconds { get; set; }
    public string ConnectionString { get; set; }
    public string BaseUrl { get; set; }
    public string WarehouseCode { get; set; }
    public string AuthPassword { get; set; }
    public string AuthUserName { get; set; }
    public string IntegrationSecretKey { get; set; }
    public int TokenExpiryInMinutes { get; set; } = 30;
}