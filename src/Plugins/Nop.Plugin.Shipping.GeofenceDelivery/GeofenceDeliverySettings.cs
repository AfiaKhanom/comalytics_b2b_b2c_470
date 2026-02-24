using Nop.Core.Configuration;

namespace Nop.Plugin.Shipping.GeofenceDelivery;

/// <summary>
/// Represents settings for the Geofence Delivery plugin
/// </summary>
public class GeofenceDeliverySettings : ISettings
{
    /// <summary>
    /// Gets or sets the Google Maps API key
    /// </summary>
    public string GoogleMapsApiKey { get; set; }

    /// <summary>
    /// Gets or sets whether the plugin is enabled
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Gets or sets whether to validate addresses on registration
    /// </summary>
    public bool ValidateOnRegistration { get; set; }

    /// <summary>
    /// Gets or sets whether to validate addresses on checkout
    /// </summary>
    public bool ValidateOnCheckout { get; set; }

    /// <summary>
    /// Gets or sets a custom error message when address is outside all zones
    /// </summary>
    public string OutsideZoneErrorMessage { get; set; }
}
