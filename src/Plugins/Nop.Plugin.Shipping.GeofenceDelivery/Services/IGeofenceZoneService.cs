using Nop.Core;
using Nop.Plugin.Shipping.GeofenceDelivery.Data.Domain;

namespace Nop.Plugin.Shipping.GeofenceDelivery.Services;

public interface IGeofenceZoneService
{
    Task<GeofenceZone> GetZoneByIdAsync(int id);
    Task<IList<GeofenceZone>> GetAllZonesAsync(bool activeOnly = true);
    Task<IPagedList<GeofenceZone>> GetPagedZonesAsync(string name = null, bool? isActive = null, int pageIndex = 0, int pageSize = int.MaxValue);
    Task InsertZoneAsync(GeofenceZone zone);
    Task UpdateZoneAsync(GeofenceZone zone);
    Task DeleteZoneAsync(GeofenceZone zone);
}
