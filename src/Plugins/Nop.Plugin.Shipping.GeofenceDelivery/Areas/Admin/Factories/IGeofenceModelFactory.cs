using Nop.Plugin.Shipping.GeofenceDelivery.Areas.Admin.Models;
using Nop.Plugin.Shipping.GeofenceDelivery.Data.Domain;

namespace Nop.Plugin.Shipping.GeofenceDelivery.Areas.Admin.Factories;

/// <summary>
/// Represents the geofence model factory interface
/// </summary>
public interface IGeofenceModelFactory
{
    Task<ConfigurationModel> PrepareConfigurationModelAsync();
    Task<GeofenceZoneListModel> PrepareGeofenceZoneListModelAsync(GeofenceZoneSearchModel searchModel);
    Task<GeofenceZoneModel> PrepareGeofenceZoneModelAsync(GeofenceZoneModel model, GeofenceZone zone);
}
