using Nop.Core;
using Nop.Plugin.Shipping.GeofenceDelivery.Data.Domain;

namespace Nop.Plugin.Shipping.GeofenceDelivery.Services;

/// <summary>
/// Represents the geofence zone service interface
/// </summary>
public interface IGeofenceZoneService
{
    Task<GeofenceZone> GetByIdAsync(int id);
    Task<IPagedList<GeofenceZone>> GetAllZonesAsync(bool showInactive = false, int pageIndex = 0, int pageSize = int.MaxValue);
    Task InsertZoneAsync(GeofenceZone zone);
    Task UpdateZoneAsync(GeofenceZone zone);
    Task DeleteZoneAsync(GeofenceZone zone);
    Task<IList<GeofenceZone>> GetActiveZonesAsync();
}
