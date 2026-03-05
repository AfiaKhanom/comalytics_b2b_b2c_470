using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Plugin.Shipping.GeofenceDelivery.Data.Domain;
using Nop.Plugin.Shipping.GeofenceDelivery.Infrastructure.Cache;

namespace Nop.Plugin.Shipping.GeofenceDelivery.Services;

/// <summary>
/// Geofence zone service
/// </summary>
public class GeofenceZoneService : IGeofenceZoneService
{
    private readonly IRepository<GeofenceZone> _geofenceZoneRepository;
    private readonly IStaticCacheManager _staticCacheManager;

    public GeofenceZoneService(IRepository<GeofenceZone> geofenceZoneRepository,
        IStaticCacheManager staticCacheManager)
    {
        _geofenceZoneRepository = geofenceZoneRepository;
        _staticCacheManager = staticCacheManager;
    }

    public async Task<GeofenceZone> GetByIdAsync(int id)
    {
        return await _geofenceZoneRepository.GetByIdAsync(id, cache => default);
    }

    public async Task<IPagedList<GeofenceZone>> GetAllZonesAsync(bool showInactive = false, int pageIndex = 0, int pageSize = int.MaxValue)
    {
        return await _geofenceZoneRepository.GetAllPagedAsync(query =>
        {
            if (!showInactive)
                query = query.Where(z => z.IsActive);

            query = query.OrderBy(z => z.DisplayOrder).ThenBy(z => z.Name);
            return query;
        }, pageIndex, pageSize);
    }

    public async Task InsertZoneAsync(GeofenceZone zone)
    {
        ArgumentNullException.ThrowIfNull(zone);
        zone.CreatedOnUtc = DateTime.UtcNow;
        zone.UpdatedOnUtc = DateTime.UtcNow;
        await _geofenceZoneRepository.InsertAsync(zone);
        await _staticCacheManager.RemoveByPrefixAsync(GeofenceCacheDefaults.ZonePrefix);
    }

    public async Task UpdateZoneAsync(GeofenceZone zone)
    {
        ArgumentNullException.ThrowIfNull(zone);
        zone.UpdatedOnUtc = DateTime.UtcNow;
        await _geofenceZoneRepository.UpdateAsync(zone);
        await _staticCacheManager.RemoveByPrefixAsync(GeofenceCacheDefaults.ZonePrefix);
    }

    public async Task DeleteZoneAsync(GeofenceZone zone)
    {
        ArgumentNullException.ThrowIfNull(zone);
        await _geofenceZoneRepository.DeleteAsync(zone);
        await _staticCacheManager.RemoveByPrefixAsync(GeofenceCacheDefaults.ZonePrefix);
    }

    public async Task<IList<GeofenceZone>> GetActiveZonesAsync()
    {
        var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(GeofenceCacheDefaults.ActiveZonesKey);
        return await _staticCacheManager.GetAsync(cacheKey, async () =>
        {
            return (await _geofenceZoneRepository.GetAllAsync(query =>
                query.Where(z => z.IsActive).OrderBy(z => z.DisplayOrder))).ToList();
        });
    }
}
