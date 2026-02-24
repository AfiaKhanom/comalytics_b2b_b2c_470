using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Shipping.GeofenceDelivery.Areas.Admin.Models;

/// <summary>
/// Represents a search model for geofence zones
/// </summary>
public record GeofenceZoneSearchModel : BaseSearchModel
{
    [NopResourceDisplayName("Plugins.Shipping.GeofenceDelivery.Fields.SearchName")]
    public string SearchName { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.GeofenceDelivery.Fields.SearchIsActive")]
    public bool? SearchIsActive { get; set; }
}
