using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Models.PartialSyncModels;

public record ErpDeliveryRoutePartialSyncModel
{
    #region Properties

    [NopResourceDisplayName("Plugin.Misc.NopStation.ErpDataScheduler.PartialSync.RouteCode")]
    public string RouteCode { get; set; }

    #endregion
}
