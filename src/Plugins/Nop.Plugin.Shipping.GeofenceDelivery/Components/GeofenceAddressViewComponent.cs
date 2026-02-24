using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Shipping.GeofenceDelivery.Services;
using Nop.Services.Configuration;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Shipping.GeofenceDelivery.Components;

/// <summary>
/// View component for geofence address validation
/// </summary>
public class GeofenceAddressViewComponent : NopViewComponent
{
    private readonly ISettingService _settingService;
    private readonly IGeofenceZoneService _geofenceZoneService;

    public GeofenceAddressViewComponent(ISettingService settingService,
        IGeofenceZoneService geofenceZoneService)
    {
        _settingService = settingService;
        _geofenceZoneService = geofenceZoneService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var settings = await _settingService.LoadSettingAsync<GeofenceDeliverySettings>();
        if (!settings.IsEnabled || string.IsNullOrEmpty(settings.GoogleMapsApiKey))
            return Content(string.Empty);

        return View("~/Plugins/Shipping.GeofenceDelivery/Views/Shared/Components/GeofenceAddress/Default.cshtml", settings.GoogleMapsApiKey);
    }
}
