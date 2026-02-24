using Nop.Plugin.Shipping.GeofenceDelivery.Models;

namespace Nop.Plugin.Shipping.GeofenceDelivery.Services;

public interface IGeofenceValidationService
{
    Task<GeofenceValidationResult> ValidateLocationAsync(decimal latitude, decimal longitude);
}
