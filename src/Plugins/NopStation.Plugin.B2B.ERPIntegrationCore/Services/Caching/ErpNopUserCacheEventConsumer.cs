using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Services.Caching;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services.Caching;

public class ErpNopUserCacheEventConsumer : CacheEventConsumer<ErpNopUser>
{
    protected override async Task ClearCacheAsync(ErpNopUser entity, EntityEventType entityEventType)
    {
        await RemoveAsync(ERPIntegrationCoreDefaults.ErpAccountByCustomerCacheKey, entity.NopCustomerId);
        await RemoveAsync(NopEntityCacheDefaults<ErpNopUser>.ByIdCacheKey, entity.Id);
        await RemoveByPrefixAsync(ERPIntegrationCoreDefaults.ErpAccountByIdWithActiveCacheKeyPrefix);
        await RemoveByPrefixAsync(ERPIntegrationCoreDefaults.ErpNopUserByCustomerCacheKeyPrefix);
    }
}
