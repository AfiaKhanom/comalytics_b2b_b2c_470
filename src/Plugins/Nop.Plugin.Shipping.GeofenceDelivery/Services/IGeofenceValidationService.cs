using Nop.Plugin.Shipping.GeofenceDelivery.Models;

namespace Nop.Plugin.Shipping.GeofenceDelivery.Services;

/// <summary>
/// Represents the geofence validation service interface
/// </summary>
public interface IGeofenceValidationService
{
    Task<GeofenceValidationResult> ValidateLocationAsync(decimal latitude, decimal longitude);
    Task<GeofenceValidationResult> ValidateAddressAsync(AddressValidationRequest request);
    bool IsPointInZone(decimal latitude, decimal longitude, IList<CoordinateDto> coordinates);
}
