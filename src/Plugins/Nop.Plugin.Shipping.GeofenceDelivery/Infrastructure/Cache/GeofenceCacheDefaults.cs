using Nop.Core.Caching;

namespace Nop.Plugin.Shipping.GeofenceDelivery.Infrastructure.Cache;

/// <summary>
/// Represents default cache keys for the Geofence Delivery plugin
/// </summary>
public static class GeofenceCacheDefaults
{
    public static readonly string ZonePrefix = "Nop.geofence.zone.";
    public static readonly CacheKey ActiveZonesKey = new CacheKey("Nop.geofence.zone.active", ZonePrefix);
    public static readonly CacheKey ZoneByIdKey = new CacheKey("Nop.geofence.zone.{0}", ZonePrefix);
    public static readonly CacheKey AllZonesKey = new CacheKey("Nop.geofence.zone.all.{0}", ZonePrefix);
}
