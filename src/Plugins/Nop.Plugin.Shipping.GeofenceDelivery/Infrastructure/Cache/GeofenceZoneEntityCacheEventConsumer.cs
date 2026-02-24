using Nop.Core.Events;
using Nop.Plugin.Shipping.GeofenceDelivery.Data.Domain;
using Nop.Services.Events;
using Nop.Core.Caching;

namespace Nop.Plugin.Shipping.GeofenceDelivery.Infrastructure.Cache;

/// <summary>
/// Cache event consumer for GeofenceZone entity
/// </summary>
public class GeofenceZoneEntityCacheEventConsumer :
    IConsumer<EntityInsertedEvent<GeofenceZone>>,
    IConsumer<EntityUpdatedEvent<GeofenceZone>>,
    IConsumer<EntityDeletedEvent<GeofenceZone>>
{
    private readonly IStaticCacheManager _staticCacheManager;

    public GeofenceZoneEntityCacheEventConsumer(IStaticCacheManager staticCacheManager)
    {
        _staticCacheManager = staticCacheManager;
    }

    public async Task HandleEventAsync(EntityInsertedEvent<GeofenceZone> eventMessage)
    {
        await _staticCacheManager.RemoveByPrefixAsync(GeofenceCacheDefaults.ZonePrefix);
    }

    public async Task HandleEventAsync(EntityUpdatedEvent<GeofenceZone> eventMessage)
    {
        await _staticCacheManager.RemoveByPrefixAsync(GeofenceCacheDefaults.ZonePrefix);
    }

    public async Task HandleEventAsync(EntityDeletedEvent<GeofenceZone> eventMessage)
    {
        await _staticCacheManager.RemoveByPrefixAsync(GeofenceCacheDefaults.ZonePrefix);
    }
}
