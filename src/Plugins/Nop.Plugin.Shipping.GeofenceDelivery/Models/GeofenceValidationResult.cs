namespace Nop.Plugin.Shipping.GeofenceDelivery.Models;

/// <summary>
/// Represents the result of a geofence validation
/// </summary>
public class GeofenceValidationResult
{
    /// <summary>
    /// Gets or sets whether the location is within a delivery zone
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Gets or sets the matched zone ID (if any)
    /// </summary>
    public int? ZoneId { get; set; }

    /// <summary>
    /// Gets or sets the matched zone name (if any)
    /// </summary>
    public string ZoneName { get; set; }

    /// <summary>
    /// Gets or sets the delivery fee for the matched zone
    /// </summary>
    public decimal DeliveryFee { get; set; }

    /// <summary>
    /// Gets or sets whether the zone is collection only
    /// </summary>
    public bool IsCollectionOnly { get; set; }

    /// <summary>
    /// Gets or sets any validation error message
    /// </summary>
    public string ErrorMessage { get; set; }
}
