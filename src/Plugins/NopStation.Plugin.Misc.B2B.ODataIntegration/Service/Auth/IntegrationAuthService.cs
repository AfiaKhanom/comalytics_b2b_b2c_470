using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Configuration;
using NopStation.Plugin.Misc.B2B.ODataIntegration.Extensions;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.Service.Auth;

public class IntegrationAuthService : IIntegrationAuthService
{
    #region Fields

    private readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;

    #endregion

    #region Ctor

    public IntegrationAuthService(
        ISettingService settingService,
        IStoreContext storeContext)
    {
        _settingService = settingService;
        _storeContext = storeContext;
    }

    #endregion

    #region Mehods

    public async Task<string> GetToken(Customer customer)
    {
        var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var now = Math.Round((DateTime.UtcNow.AddDays(180) - unixEpoch).TotalSeconds);
        var expiration = now + 30 * 60;

        var payload = new Dictionary<string, object>()
        {
            { D365IntegrationDefaults.CustomerId, customer.Id },
            { "createdon", now },
            { "exp", expiration },
        };

        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var settings = await _settingService.LoadSettingAsync<D365IntegrationSettings>(storeScope);

        return JwtHelper.JwtEncoder.Encode(
            payload,
            settings.IntegrationSecretKey
        );
    }
    #endregion
}