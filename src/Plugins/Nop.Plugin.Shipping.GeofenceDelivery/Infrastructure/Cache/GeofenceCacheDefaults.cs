using Nop.Core.Caching;

namespace Nop.Plugin.Shipping.GeofenceDelivery.Infrastructure.Cache;

public static class GeofenceCacheDefaults
{
    public static string GeofenceZonesPrefix => "Nop.geofence.zones.";
    public static CacheKey AllZonesCacheKey => new("Nop.geofence.zones.all", GeofenceZonesPrefix);
    public static CacheKey ZoneByIdCacheKey => new("Nop.geofence.zones.byid-{0}", GeofenceZonesPrefix);
}
