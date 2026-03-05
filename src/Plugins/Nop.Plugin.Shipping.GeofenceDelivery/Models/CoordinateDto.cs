namespace Nop.Plugin.Shipping.GeofenceDelivery.Models;

/// <summary>
/// Represents a geographic coordinate with sequence
/// </summary>
public class CoordinateDto
{
    /// <summary>
    /// Gets or sets the latitude
    /// </summary>
    public decimal Lat { get; set; }

    /// <summary>
    /// Gets or sets the longitude
    /// </summary>
    public decimal Lng { get; set; }

    /// <summary>
    /// Gets or sets the sequence/order
    /// </summary>
    public int Seq { get; set; }
}
