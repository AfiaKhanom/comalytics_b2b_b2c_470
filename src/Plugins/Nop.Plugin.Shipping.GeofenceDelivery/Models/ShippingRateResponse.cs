namespace Nop.Plugin.Shipping.GeofenceDelivery.Models;

/// <summary>
/// Represents a shipping rate response for a geofence zone
/// </summary>
public class ShippingRateResponse
{
    /// <summary>
    /// Gets or sets whether the rate was successfully determined
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the shipping rate
    /// </summary>
    public decimal Rate { get; set; }

    /// <summary>
    /// Gets or sets the zone name
    /// </summary>
    public string ZoneName { get; set; }

    /// <summary>
    /// Gets or sets whether this is collection only
    /// </summary>
    public bool IsCollectionOnly { get; set; }

    /// <summary>
    /// Gets or sets any error message
    /// </summary>
    public string ErrorMessage { get; set; }
}
