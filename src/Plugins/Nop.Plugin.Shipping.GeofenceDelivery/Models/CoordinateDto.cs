using System.Text.Json.Serialization;

namespace Nop.Plugin.Shipping.GeofenceDelivery.Models;

public class CoordinateDto
{
    [JsonPropertyName("lat")]
    public decimal Lat { get; set; }

    [JsonPropertyName("lng")]
    public decimal Lng { get; set; }

    [JsonPropertyName("seq")]
    public int Seq { get; set; }
}
