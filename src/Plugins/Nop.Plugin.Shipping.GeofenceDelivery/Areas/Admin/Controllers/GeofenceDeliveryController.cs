using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Nop.Plugin.Shipping.GeofenceDelivery.Areas.Admin.Models;
using Nop.Plugin.Shipping.GeofenceDelivery.Data.Domain;
using Nop.Plugin.Shipping.GeofenceDelivery.Services;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Shipping.GeofenceDelivery.Areas.Admin.Controllers;

[AuthorizeAdmin]
[Area(AreaNames.ADMIN)]
[AutoValidateAntiforgeryToken]
public class GeofenceDeliveryController : BasePluginController
{
    protected readonly IGeofenceZoneService _zoneService;
    protected readonly ISettingService _settingService;
    protected readonly ICountryService _countryService;
    protected readonly IPermissionService _permissionService;
    protected readonly ILocalizationService _localizationService;
    protected readonly INotificationService _notificationService;
    protected readonly GeofenceDeliverySettings _settings;

    public GeofenceDeliveryController(
        IGeofenceZoneService zoneService,
        ISettingService settingService,
        ICountryService countryService,
        IPermissionService permissionService,
        ILocalizationService localizationService,
        INotificationService notificationService,
        GeofenceDeliverySettings settings)
    {
        _zoneService = zoneService;
        _settingService = settingService;
        _countryService = countryService;
        _permissionService = permissionService;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _settings = settings;
    }

    public async Task<IActionResult> Configure()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AccessDeniedView();

        var countries = await _countryService.GetAllCountriesAsync();
        var model = new ConfigurationModel
        {
            Enabled = _settings.Enabled,
            DisplayName = _settings.DisplayName,
            GoogleMapsApiKey = _settings.GoogleMapsApiKey,
            OutsideZoneMessage = _settings.OutsideZoneMessage,
            BlockNonServiceableRegistration = _settings.BlockNonServiceableRegistration,
            MaxCoordinatesPerZone = _settings.MaxCoordinatesPerZone,
            TotalCoordinatesLimit = _settings.TotalCoordinatesLimit,
            DefaultCountryId = _settings.DefaultCountryId,
            AvailableCountries = countries.Select(c => new SelectListItem { Text = c.Name, Value = c.Id.ToString() }).ToList()
        };
        model.AvailableCountries.Insert(0, new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Common.All"), Value = "0" });

        return View("~/Plugins/Shipping.GeofenceDelivery/Areas/Admin/Views/Configure.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AccessDeniedView();

        _settings.Enabled = model.Enabled;
        _settings.DisplayName = model.DisplayName;
        _settings.GoogleMapsApiKey = model.GoogleMapsApiKey;
        _settings.OutsideZoneMessage = model.OutsideZoneMessage;
        _settings.BlockNonServiceableRegistration = model.BlockNonServiceableRegistration;
        _settings.MaxCoordinatesPerZone = model.MaxCoordinatesPerZone;
        _settings.TotalCoordinatesLimit = model.TotalCoordinatesLimit;
        _settings.DefaultCountryId = model.DefaultCountryId;
        await _settingService.SaveSettingAsync(_settings);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return RedirectToAction("Configure");
    }

    [HttpPost]
    public async Task<IActionResult> ZoneList(GeofenceZoneSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return await AccessDeniedDataTablesJson();

        bool? isActive = null;
        if (searchModel.SearchIsActive == 1) isActive = true;
        else if (searchModel.SearchIsActive == 2) isActive = false;

        var zones = await _zoneService.GetPagedZonesAsync(
            searchModel.SearchName,
            isActive,
            searchModel.Page - 1,
            searchModel.PageSize);

        var model = new GeofenceZoneListModel().PrepareToGrid(searchModel, zones, () =>
            zones.Select(z => new GeofenceZoneModel
            {
                Id = z.Id,
                Name = z.Name,
                IsActive = z.IsActive,
                DisplayOrder = z.DisplayOrder,
                DeliveryFee = z.DeliveryFee,
                IsCollectionOnly = z.IsCollectionOnly,
                CoordinatesJson = z.CoordinatesJson
            }));

        return Json(model);
    }

    public async Task<IActionResult> Create()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AccessDeniedView();

        var model = new GeofenceZoneModel
        {
            IsActive = true,
            CoordinatesJson = "[]"
        };

        return View("~/Plugins/Shipping.GeofenceDelivery/Areas/Admin/Views/_CreateOrUpdate.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(GeofenceZoneModel model)
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
                DeliveryFee = model.DeliveryFee,
                IsCollectionOnly = model.IsCollectionOnly,
                CoordinatesJson = model.CoordinatesJson ?? "[]"
            };
            await _zoneService.InsertZoneAsync(zone);
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));
            return RedirectToAction("Configure");
        }

        return View("~/Plugins/Shipping.GeofenceDelivery/Areas/Admin/Views/_CreateOrUpdate.cshtml", model);
    }

    public async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AccessDeniedView();

        var zone = await _zoneService.GetZoneByIdAsync(id);
        if (zone == null)
            return RedirectToAction("Configure");

        var model = new GeofenceZoneModel
        {
            Id = zone.Id,
            Name = zone.Name,
            IsActive = zone.IsActive,
            DisplayOrder = zone.DisplayOrder,
            DeliveryFee = zone.DeliveryFee,
            IsCollectionOnly = zone.IsCollectionOnly,
            CoordinatesJson = zone.CoordinatesJson
        };

        return View("~/Plugins/Shipping.GeofenceDelivery/Areas/Admin/Views/_CreateOrUpdate.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(GeofenceZoneModel model)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AccessDeniedView();

        var zone = await _zoneService.GetZoneByIdAsync(model.Id);
        if (zone == null)
            return RedirectToAction("Configure");

        if (ModelState.IsValid)
        {
            zone.Name = model.Name;
            zone.IsActive = model.IsActive;
            zone.DisplayOrder = model.DisplayOrder;
            zone.DeliveryFee = model.DeliveryFee;
            zone.IsCollectionOnly = model.IsCollectionOnly;
            zone.CoordinatesJson = model.CoordinatesJson ?? "[]";
            await _zoneService.UpdateZoneAsync(zone);
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));
            return RedirectToAction("Configure");
        }

        return View("~/Plugins/Shipping.GeofenceDelivery/Areas/Admin/Views/_CreateOrUpdate.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AccessDeniedView();

        var zone = await _zoneService.GetZoneByIdAsync(id);
        if (zone != null)
            await _zoneService.DeleteZoneAsync(zone);

        return new NullJsonResult();
    }

    [HttpPost]
    public async Task<IActionResult> ValidateLocation(decimal latitude, decimal longitude)
    {
        var validationService = HttpContext.RequestServices.GetRequiredService<IGeofenceValidationService>();
        var result = await validationService.ValidateLocationAsync(latitude, longitude);
        return Json(new
        {
            isValid = result.IsValid,
            message = result.Message,
            isCollectionOnly = result.IsCollectionOnly,
            zoneName = result.AssignedZone?.Name,
            deliveryFee = result.AssignedZone?.DeliveryFee
        });
    }
}
