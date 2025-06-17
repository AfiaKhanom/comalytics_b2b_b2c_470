using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services.Caching;

public class ErpAccountCacheEventConsumer : CacheEventConsumer<ErpAccount>
{
    protected override async Task ClearCacheAsync(ErpAccount entity, EntityEventType entityEventType)
    {
        await RemoveByPrefixAsync(ERPIntegrationCoreDefaults.ErpAccountByIdCacheKeyPrefix);
        await RemoveByPrefixAsync(ERPIntegrationCoreDefaults.ErpAccountByCustomerCacheKeyPrefix);
        await RemoveByPrefixAsync(ERPIntegrationCoreDefaults.ErpAccountByIdWithActiveCacheKeyPrefix);
    }
}