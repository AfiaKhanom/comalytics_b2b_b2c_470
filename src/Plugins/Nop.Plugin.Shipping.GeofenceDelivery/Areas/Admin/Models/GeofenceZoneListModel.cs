using Nop.Web.Framework.Models;

namespace Nop.Plugin.Shipping.GeofenceDelivery.Areas.Admin.Models;

/// <summary>
/// Represents a paged list model for geofence zones
/// </summary>
public record GeofenceZoneListModel : BasePagedListModel<GeofenceZoneModel>
{
}
