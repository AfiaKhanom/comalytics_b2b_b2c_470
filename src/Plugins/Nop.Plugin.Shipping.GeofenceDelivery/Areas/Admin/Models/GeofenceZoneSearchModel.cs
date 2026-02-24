using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Shipping.GeofenceDelivery.Areas.Admin.Models;

public record GeofenceZoneSearchModel : BaseSearchModel
{
    [NopResourceDisplayName("Plugins.Shipping.GeofenceDelivery.Zone.Name")]
    public string SearchName { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.GeofenceDelivery.Zone.IsActive")]
    public int SearchIsActive { get; set; }
}
