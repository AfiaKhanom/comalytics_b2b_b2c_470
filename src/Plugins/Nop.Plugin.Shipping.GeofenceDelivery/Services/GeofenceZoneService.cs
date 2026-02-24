using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Plugin.Shipping.GeofenceDelivery.Data.Domain;
using Nop.Plugin.Shipping.GeofenceDelivery.Infrastructure.Cache;

namespace Nop.Plugin.Shipping.GeofenceDelivery.Services;

public class GeofenceZoneService : IGeofenceZoneService
{
    protected readonly IRepository<GeofenceZone> _zoneRepository;
    protected readonly IStaticCacheManager _staticCacheManager;

    public GeofenceZoneService(
        IRepository<GeofenceZone> zoneRepository,
        IStaticCacheManager staticCacheManager)
    {
        _zoneRepository = zoneRepository;
        _staticCacheManager = staticCacheManager;
    }

    public async Task<GeofenceZone> GetZoneByIdAsync(int id)
    {
        return await _zoneRepository.GetByIdAsync(id,
            cache => cache.PrepareKeyForDefaultCache(GeofenceCacheDefaults.ZoneByIdCacheKey, id));
    }

    public async Task<IList<GeofenceZone>> GetAllZonesAsync(bool activeOnly = true)
    {
        return await _staticCacheManager.GetAsync(GeofenceCacheDefaults.AllZonesCacheKey, async () =>
        {
            return await _zoneRepository.GetAllAsync(query =>
            {
                if (activeOnly)
                    query = query.Where(z => z.IsActive);
                return query.OrderBy(z => z.DisplayOrder).ThenBy(z => z.Name);
            });
        });
    }

    public async Task<IPagedList<GeofenceZone>> GetPagedZonesAsync(string name = null, bool? isActive = null, int pageIndex = 0, int pageSize = int.MaxValue)
    {
        return await _zoneRepository.GetAllPagedAsync(query =>
        {
            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(z => z.Name.Contains(name));
            if (isActive.HasValue)
                query = query.Where(z => z.IsActive == isActive.Value);
            return query.OrderBy(z => z.DisplayOrder).ThenBy(z => z.Name);
        }, pageIndex, pageSize);
    }

    public async Task InsertZoneAsync(GeofenceZone zone)
    {
        ArgumentNullException.ThrowIfNull(zone);
        zone.CreatedOnUtc = DateTime.UtcNow;
        zone.UpdatedOnUtc = DateTime.UtcNow;
        await _zoneRepository.InsertAsync(zone);
        await _staticCacheManager.RemoveByPrefixAsync(GeofenceCacheDefaults.GeofenceZonesPrefix);
    }

    public async Task UpdateZoneAsync(GeofenceZone zone)
    {
        ArgumentNullException.ThrowIfNull(zone);
        zone.UpdatedOnUtc = DateTime.UtcNow;
        await _zoneRepository.UpdateAsync(zone);
        await _staticCacheManager.RemoveByPrefixAsync(GeofenceCacheDefaults.GeofenceZonesPrefix);
    }

    public async Task DeleteZoneAsync(GeofenceZone zone)
    {
        ArgumentNullException.ThrowIfNull(zone);
        await _zoneRepository.DeleteAsync(zone);
        await _staticCacheManager.RemoveByPrefixAsync(GeofenceCacheDefaults.GeofenceZonesPrefix);
    }
}
