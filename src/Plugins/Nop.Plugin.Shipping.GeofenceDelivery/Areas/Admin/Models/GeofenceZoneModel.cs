using Nop.Plugin.Shipping.GeofenceDelivery.Models;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Shipping.GeofenceDelivery.Areas.Admin.Models;

public record GeofenceZoneModel : BaseNopEntityModel
{
    [NopResourceDisplayName("Plugins.Shipping.GeofenceDelivery.Zone.Name")]
    public string Name { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.GeofenceDelivery.Zone.IsActive")]
    public bool IsActive { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.GeofenceDelivery.Zone.DisplayOrder")]
    public int DisplayOrder { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.GeofenceDelivery.Zone.DeliveryFee")]
    public decimal DeliveryFee { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.GeofenceDelivery.Zone.IsCollectionOnly")]
    public bool IsCollectionOnly { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.GeofenceDelivery.Zone.CoordinatesJson")]
    public string CoordinatesJson { get; set; }

    public IList<CoordinateDto> Coordinates { get; set; } = new List<CoordinateDto>();
}
