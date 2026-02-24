using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Shipping.GeofenceDelivery.Areas.Admin.Models;

/// <summary>
/// Represents a geofence zone model
/// </summary>
public record GeofenceZoneModel : BaseNopEntityModel
{
    [NopResourceDisplayName("Plugins.Shipping.GeofenceDelivery.Fields.Name")]
    public string Name { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.GeofenceDelivery.Fields.IsActive")]
    public bool IsActive { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.GeofenceDelivery.Fields.DisplayOrder")]
    public int DisplayOrder { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.GeofenceDelivery.Fields.CoordinatesJson")]
    public string CoordinatesJson { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.GeofenceDelivery.Fields.DeliveryFee")]
    public decimal DeliveryFee { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.GeofenceDelivery.Fields.IsCollectionOnly")]
    public bool IsCollectionOnly { get; set; }
}
