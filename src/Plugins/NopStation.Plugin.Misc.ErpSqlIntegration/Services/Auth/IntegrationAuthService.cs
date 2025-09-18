using Nop.Core.Domain.Customers;
using NopStation.Plugin.Misc.ErpSqlIntegration.Extensions;

namespace NopStation.Plugin.Misc.ErpSqlIntegration.Services.Auth;

public class IntegrationAuthService : IIntegrationAuthService
{
    #region Fields

    private readonly  SqlIntegrationSettings _settings;

    #endregion

    #region Ctor

    public IntegrationAuthService(SqlIntegrationSettings settings)
    {
        _settings = settings;
    }

    #endregion

    #region Mehods

    public string GetToken(Customer customer)
    {
        var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var now = Math.Round((DateTime.UtcNow.AddDays(180) - unixEpoch).TotalSeconds);
        var expiration = now + _settings.TokenExpiryInMinutes * 60;

        var payload = new Dictionary<string, object>()
        {
            { SqlIntegrationDefaults.CustomerId, customer.Id },
            { "createdon", now },
            { "exp", expiration },
        };

        return JwtHelper.JwtEncoder.Encode(
            payload,
            _settings.IntegrationSecretKey
        );
    }
    #endregion
}