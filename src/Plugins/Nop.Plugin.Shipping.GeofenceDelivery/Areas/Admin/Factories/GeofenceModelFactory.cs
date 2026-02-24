using Nop.Plugin.Shipping.GeofenceDelivery.Areas.Admin.Models;
using Nop.Plugin.Shipping.GeofenceDelivery.Data.Domain;
using Nop.Plugin.Shipping.GeofenceDelivery.Services;
using Nop.Services.Configuration;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Shipping.GeofenceDelivery.Areas.Admin.Factories;

/// <summary>
/// Geofence model factory
/// </summary>
public class GeofenceModelFactory : IGeofenceModelFactory
{
    private readonly IGeofenceZoneService _geofenceZoneService;
    private readonly ISettingService _settingService;

    public GeofenceModelFactory(IGeofenceZoneService geofenceZoneService,
        ISettingService settingService)
    {
        _geofenceZoneService = geofenceZoneService;
        _settingService = settingService;
    }

    public async Task<ConfigurationModel> PrepareConfigurationModelAsync()
    {
        var settings = await _settingService.LoadSettingAsync<GeofenceDeliverySettings>();
        var model = new ConfigurationModel
        {
            GoogleMapsApiKey = settings.GoogleMapsApiKey,
            IsEnabled = settings.IsEnabled,
            ValidateOnRegistration = settings.ValidateOnRegistration,
            ValidateOnCheckout = settings.ValidateOnCheckout,
            OutsideZoneErrorMessage = settings.OutsideZoneErrorMessage,
            GeofenceZoneSearchModel = new GeofenceZoneSearchModel()
        };
        model.GeofenceZoneSearchModel.SetGridPageSize();
        return model;
    }

    public async Task<GeofenceZoneListModel> PrepareGeofenceZoneListModelAsync(GeofenceZoneSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var zones = await _geofenceZoneService.GetAllZonesAsync(
            showInactive: true,
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize);

        var model = await new GeofenceZoneListModel().PrepareToGridAsync(searchModel, zones, () =>
        {
            return zones.ToAsyncEnumerable().SelectAwait(async zone => await PrepareGeofenceZoneModelAsync(null, zone));
        });

        return model;
    }

    public Task<GeofenceZoneModel> PrepareGeofenceZoneModelAsync(GeofenceZoneModel model, GeofenceZone zone)
    {
        if (zone != null)
        {
            model ??= new GeofenceZoneModel();
            model.Id = zone.Id;
            model.Name = zone.Name;
            model.IsActive = zone.IsActive;
            model.DisplayOrder = zone.DisplayOrder;
            model.CoordinatesJson = zone.CoordinatesJson;
            model.DeliveryFee = zone.DeliveryFee;
            model.IsCollectionOnly = zone.IsCollectionOnly;
        }

        return Task.FromResult(model ?? new GeofenceZoneModel { IsActive = true });
    }
}
