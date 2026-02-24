using Nop.Core.Configuration;

namespace Nop.Plugin.Shipping.GeofenceDelivery;

public class GeofenceDeliverySettings : ISettings
{
    public bool Enabled { get; set; }
    public string DisplayName { get; set; } = "Geofence Delivery";
    public string GoogleMapsApiKey { get; set; }
    public string OutsideZoneMessage { get; set; } = "Delivery is not available to your location.";
    public bool BlockNonServiceableRegistration { get; set; }
    public int MaxCoordinatesPerZone { get; set; } = 5000;
    public int TotalCoordinatesLimit { get; set; } = 20000;
    public int DefaultCountryId { get; set; }
}
