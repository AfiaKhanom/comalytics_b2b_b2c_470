using Nop.Core.Domain.Shipping;
using Nop.Plugin.Shipping.GeofenceDelivery.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Tracking;
using Nop.Core;

namespace Nop.Plugin.Shipping.GeofenceDelivery;

/// <summary>
/// Geofence-based delivery shipping computation method
/// </summary>
public class GeofenceDeliveryComputationMethod : BasePlugin, IShippingRateComputationMethod
{
    private readonly GeofenceDeliverySettings _geofenceDeliverySettings;
    private readonly IGeofenceValidationService _geofenceValidationService;
    private readonly ILocalizationService _localizationService;
    private readonly ISettingService _settingService;
    private readonly IWebHelper _webHelper;

    public GeofenceDeliveryComputationMethod(GeofenceDeliverySettings geofenceDeliverySettings,
        IGeofenceValidationService geofenceValidationService,
        ILocalizationService localizationService,
        ISettingService settingService,
        IWebHelper webHelper)
    {
        _geofenceDeliverySettings = geofenceDeliverySettings;
        _geofenceValidationService = geofenceValidationService;
        _localizationService = localizationService;
        _settingService = settingService;
        _webHelper = webHelper;
    }

    public async Task<GetShippingOptionResponse> GetShippingOptionsAsync(GetShippingOptionRequest getShippingOptionRequest)
    {
        ArgumentNullException.ThrowIfNull(getShippingOptionRequest);

        var response = new GetShippingOptionResponse();

        if (!_geofenceDeliverySettings.IsEnabled)
        {
            response.AddError("Geofence Delivery plugin is not enabled");
            return response;
        }

        if (getShippingOptionRequest.Items == null || !getShippingOptionRequest.Items.Any())
        {
            response.AddError("No shipment items");
            return response;
        }

        if (getShippingOptionRequest.ShippingAddress == null)
        {
            response.AddError("Shipping address is not set");
            return response;
        }

        var validationRequest = new Models.AddressValidationRequest
        {
            Address1 = getShippingOptionRequest.ShippingAddress.Address1,
            City = getShippingOptionRequest.ShippingAddress.City,
            ZipPostalCode = getShippingOptionRequest.ShippingAddress.ZipPostalCode
        };

        var validationResult = await _geofenceValidationService.ValidateAddressAsync(validationRequest);

        if (!validationResult.IsValid)
        {
            response.AddError(validationResult.ErrorMessage ??
                await _localizationService.GetResourceAsync("Plugins.Shipping.GeofenceDelivery.OutsideZone"));
            return response;
        }

        var shippingOptionName = validationResult.IsCollectionOnly
            ? await _localizationService.GetResourceAsync("Plugins.Shipping.GeofenceDelivery.CollectionOnly")
            : await _localizationService.GetResourceAsync("Plugins.Shipping.GeofenceDelivery.DeliveryToZone");

        response.ShippingOptions.Add(new ShippingOption
        {
            Name = $"{shippingOptionName} ({validationResult.ZoneName})",
            Description = validationResult.IsCollectionOnly
                ? await _localizationService.GetResourceAsync("Plugins.Shipping.GeofenceDelivery.CollectionOnlyDescription")
                : string.Format(await _localizationService.GetResourceAsync("Plugins.Shipping.GeofenceDelivery.DeliveryZoneDescription"), validationResult.ZoneName),
            Rate = validationResult.DeliveryFee
        });

        return response;
    }

    public Task<decimal?> GetFixedRateAsync(GetShippingOptionRequest getShippingOptionRequest)
    {
        return Task.FromResult<decimal?>(null);
    }

    public Task<IShipmentTracker> GetShipmentTrackerAsync()
    {
        return Task.FromResult<IShipmentTracker>(null);
    }

    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/GeofenceDelivery/Configure";
    }

    public override async Task InstallAsync()
    {
        await _settingService.SaveSettingAsync(new GeofenceDeliverySettings
        {
            IsEnabled = true,
            ValidateOnRegistration = false,
            ValidateOnCheckout = true,
            OutsideZoneErrorMessage = "Sorry, we don't deliver to your area."
        });

        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Plugins.Shipping.GeofenceDelivery.Fields.GoogleMapsApiKey"] = "Google Maps API Key",
            ["Plugins.Shipping.GeofenceDelivery.Fields.GoogleMapsApiKey.Hint"] = "Enter your Google Maps API key for geocoding and map display.",
            ["Plugins.Shipping.GeofenceDelivery.Fields.GoogleMapsApiKey.Required"] = "Google Maps API Key is required",
            ["Plugins.Shipping.GeofenceDelivery.Fields.IsEnabled"] = "Enabled",
            ["Plugins.Shipping.GeofenceDelivery.Fields.IsEnabled.Hint"] = "Check to enable the Geofence Delivery plugin.",
            ["Plugins.Shipping.GeofenceDelivery.Fields.ValidateOnRegistration"] = "Validate on Registration",
            ["Plugins.Shipping.GeofenceDelivery.Fields.ValidateOnRegistration.Hint"] = "Check to validate address during customer registration.",
            ["Plugins.Shipping.GeofenceDelivery.Fields.ValidateOnCheckout"] = "Validate on Checkout",
            ["Plugins.Shipping.GeofenceDelivery.Fields.ValidateOnCheckout.Hint"] = "Check to validate delivery address during checkout.",
            ["Plugins.Shipping.GeofenceDelivery.Fields.OutsideZoneErrorMessage"] = "Outside Zone Error Message",
            ["Plugins.Shipping.GeofenceDelivery.Fields.OutsideZoneErrorMessage.Hint"] = "Error message displayed when address is outside all delivery zones.",
            ["Plugins.Shipping.GeofenceDelivery.Fields.Name"] = "Name",
            ["Plugins.Shipping.GeofenceDelivery.Fields.Name.Required"] = "Zone name is required",
            ["Plugins.Shipping.GeofenceDelivery.Fields.IsActive"] = "Active",
            ["Plugins.Shipping.GeofenceDelivery.Fields.DisplayOrder"] = "Display Order",
            ["Plugins.Shipping.GeofenceDelivery.Fields.CoordinatesJson"] = "Coordinates (JSON)",
            ["Plugins.Shipping.GeofenceDelivery.Fields.CoordinatesJson.Required"] = "Zone coordinates are required",
            ["Plugins.Shipping.GeofenceDelivery.Fields.DeliveryFee"] = "Delivery Fee",
            ["Plugins.Shipping.GeofenceDelivery.Fields.DeliveryFee.Invalid"] = "Delivery fee must be 0 or greater",
            ["Plugins.Shipping.GeofenceDelivery.Fields.IsCollectionOnly"] = "Collection Only",
            ["Plugins.Shipping.GeofenceDelivery.Fields.SearchName"] = "Zone Name",
            ["Plugins.Shipping.GeofenceDelivery.Fields.SearchIsActive"] = "Active",
            ["Plugins.Shipping.GeofenceDelivery.Zone.Created"] = "Delivery zone created successfully.",
            ["Plugins.Shipping.GeofenceDelivery.Zone.Updated"] = "Delivery zone updated successfully.",
            ["Plugins.Shipping.GeofenceDelivery.Zone.Deleted"] = "Delivery zone deleted successfully.",
            ["Plugins.Shipping.GeofenceDelivery.OutsideZone"] = "Sorry, we do not deliver to your area.",
            ["Plugins.Shipping.GeofenceDelivery.CollectionOnly"] = "Collection Only",
            ["Plugins.Shipping.GeofenceDelivery.CollectionOnlyDescription"] = "This item is available for collection only.",
            ["Plugins.Shipping.GeofenceDelivery.DeliveryToZone"] = "Delivery",
            ["Plugins.Shipping.GeofenceDelivery.DeliveryZoneDescription"] = "Delivery to {0} zone.",
            ["Plugins.Shipping.GeofenceDelivery.AdminMenu"] = "Geofence Delivery",
            ["Plugins.Shipping.GeofenceDelivery.Configuration"] = "Configuration",
            ["Plugins.Shipping.GeofenceDelivery.Zones"] = "Delivery Zones",
            ["Plugins.Shipping.GeofenceDelivery.AddNewZone"] = "Add New Zone",
            ["Plugins.Shipping.GeofenceDelivery.BackToConfiguration"] = "Back to Configuration"
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
