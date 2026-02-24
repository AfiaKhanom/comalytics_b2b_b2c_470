using Nop.Core;

namespace Nop.Plugin.Shipping.GeofenceDelivery.Data.Domain;

/// <summary>
/// Represents a geofence delivery zone
/// </summary>
public class GeofenceZone : BaseEntity
{
    /// <summary>
    /// Gets or sets the zone name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets whether the zone is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the display order
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Gets or sets the coordinates as JSON string
    /// Format: [{"lat":40.7,"lng":-74.0,"seq":0},...]
    /// </summary>
    public string CoordinatesJson { get; set; }

    /// <summary>
    /// Gets or sets the delivery fee for this zone
    /// </summary>
    public decimal DeliveryFee { get; set; }

    /// <summary>
    /// Gets or sets whether this zone is collection only (no delivery)
    /// </summary>
    public bool IsCollectionOnly { get; set; }

    /// <summary>
    /// Gets or sets the creation date (UTC)
    /// </summary>
    public DateTime CreatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the last update date (UTC)
    /// </summary>
    public DateTime UpdatedOnUtc { get; set; }
}
