using Nop.Core;

namespace Nop.Plugin.Shipping.GeofenceDelivery.Data.Domain;

public class GeofenceZone : BaseEntity
{
    public string Name { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public string CoordinatesJson { get; set; }
    public decimal DeliveryFee { get; set; }
    public bool IsCollectionOnly { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime UpdatedOnUtc { get; set; }
}
