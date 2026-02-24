using FluentValidation;
using Nop.Plugin.Shipping.GeofenceDelivery.Areas.Admin.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Shipping.GeofenceDelivery.Areas.Admin.Validators;

/// <summary>
/// Validator for the GeofenceZoneModel
/// </summary>
public class GeofenceZoneValidator : BaseNopValidator<GeofenceZoneModel>
{
    public GeofenceZoneValidator(ILocalizationService localizationService)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Shipping.GeofenceDelivery.Fields.Name.Required"));

        RuleFor(x => x.DeliveryFee)
            .GreaterThanOrEqualTo(0)
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Shipping.GeofenceDelivery.Fields.DeliveryFee.Invalid"));

        RuleFor(x => x.CoordinatesJson)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Shipping.GeofenceDelivery.Fields.CoordinatesJson.Required"));
    }
}
