using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Shipping.GeofenceDelivery.Areas.Admin.Models;

public record ConfigurationModel : BaseNopModel
{
    [NopResourceDisplayName("Plugins.Shipping.GeofenceDelivery.Fields.Enabled")]
    public bool Enabled { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.GeofenceDelivery.Fields.DisplayName")]
    public string DisplayName { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.GeofenceDelivery.Fields.GoogleMapsApiKey")]
    public string GoogleMapsApiKey { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.GeofenceDelivery.Fields.OutsideZoneMessage")]
    public string OutsideZoneMessage { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.GeofenceDelivery.Fields.BlockNonServiceableRegistration")]
    public bool BlockNonServiceableRegistration { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.GeofenceDelivery.Fields.MaxCoordinatesPerZone")]
    public int MaxCoordinatesPerZone { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.GeofenceDelivery.Fields.TotalCoordinatesLimit")]
    public int TotalCoordinatesLimit { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.GeofenceDelivery.Fields.DefaultCountry")]
    public int DefaultCountryId { get; set; }
    public IList<SelectListItem> AvailableCountries { get; set; } = new List<SelectListItem>();
}
