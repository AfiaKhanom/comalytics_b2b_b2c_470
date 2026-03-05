using Nop.Plugin.Shipping.GeofenceDelivery.Models;

namespace Nop.Plugin.Shipping.GeofenceDelivery.Services;

/// <summary>
/// Represents the Google Maps service interface
/// </summary>
public interface IGoogleMapsService
{
    Task<LocationValidationResult> GeocodeAddressAsync(string address);
}
