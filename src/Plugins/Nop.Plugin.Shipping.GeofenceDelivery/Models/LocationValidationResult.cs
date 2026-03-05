namespace Nop.Plugin.Shipping.GeofenceDelivery.Models;

/// <summary>
/// Represents the result of a location/address lookup
/// </summary>
public class LocationValidationResult
{
    /// <summary>
    /// Gets or sets whether geocoding was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the latitude
    /// </summary>
    public decimal Latitude { get; set; }

    /// <summary>
    /// Gets or sets the longitude
    /// </summary>
    public decimal Longitude { get; set; }

    /// <summary>
    /// Gets or sets the formatted address returned by geocoding
    /// </summary>
    public string FormattedAddress { get; set; }

    /// <summary>
    /// Gets or sets any error message
    /// </summary>
    public string ErrorMessage { get; set; }
}
