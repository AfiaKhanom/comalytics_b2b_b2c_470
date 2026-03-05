using Nop.Core;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Shipping.GeofenceDelivery.Components;
using Nop.Plugin.Shipping.GeofenceDelivery.Services;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Tracking;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Shipping.GeofenceDelivery;

public class GeofenceDeliveryPlugin : BasePlugin, IShippingRateComputationMethod, IWidgetPlugin
{
    protected readonly GeofenceDeliverySettings _settings;
    protected readonly IGeofenceValidationService _validationService;
    protected readonly ILocalizationService _localizationService;
    protected readonly ISettingService _settingService;
    protected readonly IWebHelper _webHelper;

    public GeofenceDeliveryPlugin(
        GeofenceDeliverySettings settings,
        IGeofenceValidationService validationService,
        ILocalizationService localizationService,
        ISettingService settingService,
        IWebHelper webHelper)
    {
        _settings = settings;
        _validationService = validationService;
        _localizationService = localizationService;
        _settingService = settingService;
        _webHelper = webHelper;
    }

    public bool HideInWidgetList => true;

    public override string GetConfigurationPageUrl()
    {
        return _webHelper.GetStoreLocation() + "Admin/GeofenceDelivery/Configure";
    }

    public Task<IList<string>> GetWidgetZonesAsync()
    {
        return Task.FromResult<IList<string>>(new List<string>
        {
            PublicWidgetZones.CheckoutBillingAddressBottom,
            PublicWidgetZones.CheckoutShippingAddressBottom,
            PublicWidgetZones.AddressBottom
        });
    }

    public Type GetWidgetViewComponent(string widgetZone)
    {
        return typeof(GeofenceAddressWidgetViewComponent);
    }

    public Task<GetShippingOptionResponse> GetShippingOptionsAsync(GetShippingOptionRequest request)
    {
        var response = new GetShippingOptionResponse();

        if (!_settings.Enabled)
            return Task.FromResult(response);

        var address = request.ShippingAddress;
        if (address == null)
        {
            response.Errors.Add("Address not found. Please validate your address.");
            return Task.FromResult(response);
        }

        response.Errors.Add(_settings.OutsideZoneMessage);
        return Task.FromResult(response);
    }

    public Task<decimal?> GetFixedRateAsync(GetShippingOptionRequest getShippingOptionRequest)
    {
        return Task.FromResult<decimal?>(null);
    }

    public Task<IShipmentTracker> GetShipmentTrackerAsync()
    {
        return Task.FromResult<IShipmentTracker>(null);
    }

    public override async Task InstallAsync()
    {
        await _settingService.SaveSettingAsync(new GeofenceDeliverySettings
        {
            Enabled = false,
            DisplayName = "Geofence Delivery",
            OutsideZoneMessage = "Delivery is not available to your location.",
            MaxCoordinatesPerZone = 5000,
            TotalCoordinatesLimit = 20000
        });

        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Plugins.Shipping.GeofenceDelivery.Configure"] = "Geofence Delivery Configuration",
            ["Plugins.Shipping.GeofenceDelivery.Fields.Enabled"] = "Enabled",
            ["Plugins.Shipping.GeofenceDelivery.Fields.Enabled.Hint"] = "Enable or disable the geofence delivery plugin.",
            ["Plugins.Shipping.GeofenceDelivery.Fields.DisplayName"] = "Display Name",
            ["Plugins.Shipping.GeofenceDelivery.Fields.DisplayName.Hint"] = "The name shown at checkout.",
            ["Plugins.Shipping.GeofenceDelivery.Fields.GoogleMapsApiKey"] = "Google Maps API Key",
            ["Plugins.Shipping.GeofenceDelivery.Fields.GoogleMapsApiKey.Hint"] = "Enter your Google Maps API Key.",
            ["Plugins.Shipping.GeofenceDelivery.Fields.OutsideZoneMessage"] = "Outside Zone Message",
            ["Plugins.Shipping.GeofenceDelivery.Fields.OutsideZoneMessage.Hint"] = "Message shown when delivery is unavailable.",
            ["Plugins.Shipping.GeofenceDelivery.Fields.BlockNonServiceableRegistration"] = "Block Non-Serviceable Registration",
            ["Plugins.Shipping.GeofenceDelivery.Fields.BlockNonServiceableRegistration.Hint"] = "Block registration if address is outside delivery zones.",
            ["Plugins.Shipping.GeofenceDelivery.Fields.MaxCoordinatesPerZone"] = "Max Coordinates Per Zone",
            ["Plugins.Shipping.GeofenceDelivery.Fields.MaxCoordinatesPerZone.Hint"] = "Maximum number of coordinates per zone.",
            ["Plugins.Shipping.GeofenceDelivery.Fields.TotalCoordinatesLimit"] = "Total Coordinates Limit",
            ["Plugins.Shipping.GeofenceDelivery.Fields.TotalCoordinatesLimit.Hint"] = "Total coordinates limit across all zones.",
            ["Plugins.Shipping.GeofenceDelivery.Fields.DefaultCountry"] = "Default Country",
            ["Plugins.Shipping.GeofenceDelivery.Zones"] = "Delivery Zones",
            ["Plugins.Shipping.GeofenceDelivery.Zone.Name"] = "Zone Name",
            ["Plugins.Shipping.GeofenceDelivery.Zone.IsActive"] = "Active",
            ["Plugins.Shipping.GeofenceDelivery.Zone.DisplayOrder"] = "Display Order",
            ["Plugins.Shipping.GeofenceDelivery.Zone.DeliveryFee"] = "Delivery Fee",
            ["Plugins.Shipping.GeofenceDelivery.Zone.IsCollectionOnly"] = "Collection Only",
            ["Plugins.Shipping.GeofenceDelivery.Zone.CoordinatesJson"] = "Coordinates (JSON)",
            ["Plugins.Shipping.GeofenceDelivery.Zone.CoordinatesJson.Hint"] = "JSON array of coordinates: [{\"lat\":40.71,\"lng\":-74.00,\"seq\":0},...]",
            ["Plugins.Shipping.GeofenceDelivery.Zone.Add"] = "Add New Zone",
            ["Plugins.Shipping.GeofenceDelivery.Zone.Edit"] = "Edit Zone",
            ["Plugins.Shipping.GeofenceDelivery.Zone.BasicInfo"] = "Basic Information",
            ["Plugins.Shipping.GeofenceDelivery.Zone.Boundaries"] = "Geographic Boundaries",
            ["Plugins.Shipping.GeofenceDelivery.Address.DeliveryAvailable"] = "✓ Delivery available to this area",
            ["Plugins.Shipping.GeofenceDelivery.Address.NotServiceable"] = "✗ Delivery unavailable to this address",
            ["Plugins.Shipping.GeofenceDelivery.UseCurrentLocation"] = "Use my current location",
            ["Plugins.Shipping.GeofenceDelivery.CoordinatesNotSet"] = "Coordinates: Not set"
        });

        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        await _settingService.DeleteSettingAsync<GeofenceDeliverySettings>();
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Shipping.GeofenceDelivery");
        await base.UninstallAsync();
    }
}
