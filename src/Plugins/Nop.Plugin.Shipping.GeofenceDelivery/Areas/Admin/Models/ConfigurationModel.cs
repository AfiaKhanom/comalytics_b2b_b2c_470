using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Shipping.GeofenceDelivery.Areas.Admin.Models;

/// <summary>
/// Represents the plugin configuration model
/// </summary>
public record ConfigurationModel : BaseNopModel
{
    [NopResourceDisplayName("Plugins.Shipping.GeofenceDelivery.Fields.GoogleMapsApiKey")]
    public string GoogleMapsApiKey { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.GeofenceDelivery.Fields.IsEnabled")]
    public bool IsEnabled { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.GeofenceDelivery.Fields.ValidateOnRegistration")]
    public bool ValidateOnRegistration { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.GeofenceDelivery.Fields.ValidateOnCheckout")]
    public bool ValidateOnCheckout { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.GeofenceDelivery.Fields.OutsideZoneErrorMessage")]
    public string OutsideZoneErrorMessage { get; set; }

    public GeofenceZoneSearchModel GeofenceZoneSearchModel { get; set; }
}
