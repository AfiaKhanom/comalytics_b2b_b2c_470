namespace Nop.Plugin.Shipping.GeofenceDelivery.Models;

/// <summary>
/// Represents a request to validate an address against geofence zones
/// </summary>
public class AddressValidationRequest
{
    /// <summary>
    /// Gets or sets the street address
    /// </summary>
    public string Address1 { get; set; }

    /// <summary>
    /// Gets or sets the city
    /// </summary>
    public string City { get; set; }

    /// <summary>
    /// Gets or sets the zip/postal code
    /// </summary>
    public string ZipPostalCode { get; set; }

    /// <summary>
    /// Gets or sets the country name or code
    /// </summary>
    public string Country { get; set; }

    /// <summary>
    /// Gets or sets the latitude (if already geocoded)
    /// </summary>
    public decimal? Latitude { get; set; }

    /// <summary>
    /// Gets or sets the longitude (if already geocoded)
    /// </summary>
    public decimal? Longitude { get; set; }
}
