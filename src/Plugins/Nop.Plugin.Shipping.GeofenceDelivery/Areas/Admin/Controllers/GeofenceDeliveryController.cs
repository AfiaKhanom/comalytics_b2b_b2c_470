using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Shipping.GeofenceDelivery.Areas.Admin.Factories;
using Nop.Plugin.Shipping.GeofenceDelivery.Areas.Admin.Models;
using Nop.Plugin.Shipping.GeofenceDelivery.Data.Domain;
using Nop.Plugin.Shipping.GeofenceDelivery.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Shipping.GeofenceDelivery.Areas.Admin.Controllers;

[AuthorizeAdmin]
[Area(AreaNames.ADMIN)]
[AutoValidateAntiforgeryToken]
public class GeofenceDeliveryController : BasePluginController
{
    private readonly IGeofenceModelFactory _geofenceModelFactory;
    private readonly IGeofenceZoneService _geofenceZoneService;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly IPermissionService _permissionService;
    private readonly ISettingService _settingService;

    public GeofenceDeliveryController(IGeofenceModelFactory geofenceModelFactory,
        IGeofenceZoneService geofenceZoneService,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IPermissionService permissionService,
        ISettingService settingService)
    {
        _geofenceModelFactory = geofenceModelFactory;
        _geofenceZoneService = geofenceZoneService;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _permissionService = permissionService;
        _settingService = settingService;
    }

    public async Task<IActionResult> Configure()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AccessDeniedView();

        var model = await _geofenceModelFactory.PrepareConfigurationModelAsync();
        return View("~/Plugins/Shipping.GeofenceDelivery/Areas/Admin/Views/Configure.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AccessDeniedView();

        var settings = await _settingService.LoadSettingAsync<GeofenceDeliverySettings>();
        settings.GoogleMapsApiKey = model.GoogleMapsApiKey;
        settings.IsEnabled = model.IsEnabled;
        settings.ValidateOnRegistration = model.ValidateOnRegistration;
        settings.ValidateOnCheckout = model.ValidateOnCheckout;
        settings.OutsideZoneErrorMessage = model.OutsideZoneErrorMessage;
        await _settingService.SaveSettingAsync(settings);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return RedirectToAction("Configure");
    }

    [HttpPost]
    public async Task<IActionResult> ZoneList(GeofenceZoneSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return await AccessDeniedDataTablesJson();

        var model = await _geofenceModelFactory.PrepareGeofenceZoneListModelAsync(searchModel);
        return Json(model);
    }

    public async Task<IActionResult> CreateZone()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AccessDeniedView();

        var model = await _geofenceModelFactory.PrepareGeofenceZoneModelAsync(null, null);
        return View("~/Plugins/Shipping.GeofenceDelivery/Areas/Admin/Views/_CreateOrUpdate.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> CreateZone(GeofenceZoneModel model)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AccessDeniedView();

        if (ModelState.IsValid)
        {
            var zone = new GeofenceZone
            {
                Name = model.Name,
                IsActive = model.IsActive,
                DisplayOrder = model.DisplayOrder,
                CoordinatesJson = model.CoordinatesJson,
                DeliveryFee = model.DeliveryFee,
                IsCollectionOnly = model.IsCollectionOnly
            };
            await _geofenceZoneService.InsertZoneAsync(zone);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Shipping.GeofenceDelivery.Zone.Created"));
            return RedirectToAction("Configure");
        }

        return View("~/Plugins/Shipping.GeofenceDelivery/Areas/Admin/Views/_CreateOrUpdate.cshtml", model);
    }

    public async Task<IActionResult> EditZone(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AccessDeniedView();

        var zone = await _geofenceZoneService.GetByIdAsync(id);
        if (zone == null)
            return RedirectToAction("Configure");

        var model = await _geofenceModelFactory.PrepareGeofenceZoneModelAsync(null, zone);
        return View("~/Plugins/Shipping.GeofenceDelivery/Areas/Admin/Views/_CreateOrUpdate.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> EditZone(GeofenceZoneModel model)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AccessDeniedView();

        var zone = await _geofenceZoneService.GetByIdAsync(model.Id);
        if (zone == null)
            return RedirectToAction("Configure");

        if (ModelState.IsValid)
        {
            zone.Name = model.Name;
            zone.IsActive = model.IsActive;
            zone.DisplayOrder = model.DisplayOrder;
            zone.CoordinatesJson = model.CoordinatesJson;
            zone.DeliveryFee = model.DeliveryFee;
            zone.IsCollectionOnly = model.IsCollectionOnly;
            await _geofenceZoneService.UpdateZoneAsync(zone);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Shipping.GeofenceDelivery.Zone.Updated"));
            return RedirectToAction("Configure");
        }

        return View("~/Plugins/Shipping.GeofenceDelivery/Areas/Admin/Views/_CreateOrUpdate.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteZone(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AccessDeniedView();

        var zone = await _geofenceZoneService.GetByIdAsync(id);
        if (zone != null)
        {
            await _geofenceZoneService.DeleteZoneAsync(zone);
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Shipping.GeofenceDelivery.Zone.Deleted"));
        }

        return RedirectToAction("Configure");
    }
}
