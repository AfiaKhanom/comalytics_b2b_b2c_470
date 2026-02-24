using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Shipping.GeofenceDelivery.Services;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Shipping.GeofenceDelivery.Components;

public class GeofenceAddressWidgetViewComponent : NopViewComponent
{
    protected readonly GeofenceDeliverySettings _settings;
    protected readonly IGeofenceZoneService _zoneService;

    public GeofenceAddressWidgetViewComponent(
        GeofenceDeliverySettings settings,
        IGeofenceZoneService zoneService)
    {
        _settings = settings;
        _zoneService = zoneService;
    }

    public IViewComponentResult InvokeAsync(string widgetZone, object additionalData)
    {
        if (!_settings.Enabled || string.IsNullOrEmpty(_settings.GoogleMapsApiKey))
            return Content(string.Empty);

        return View("~/Plugins/Shipping.GeofenceDelivery/Components/Views/Default.cshtml", _settings);
    }
}
