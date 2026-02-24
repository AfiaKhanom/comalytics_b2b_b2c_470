using Nop.Plugin.Shipping.GeofenceDelivery.Data.Domain;

namespace Nop.Plugin.Shipping.GeofenceDelivery.Models;

public class GeofenceValidationResult
{
    public bool IsValid { get; set; }
    public string Message { get; set; }
    public GeofenceZone AssignedZone { get; set; }
    public bool IsCollectionOnly { get; set; }
    public bool IsLocationFlagged { get; set; }
}
