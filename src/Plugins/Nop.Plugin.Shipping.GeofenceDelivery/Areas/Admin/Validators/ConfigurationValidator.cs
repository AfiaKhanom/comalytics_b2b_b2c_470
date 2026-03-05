using FluentValidation;
using Nop.Plugin.Shipping.GeofenceDelivery.Areas.Admin.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Shipping.GeofenceDelivery.Areas.Admin.Validators;

/// <summary>
/// Validator for the ConfigurationModel
/// </summary>
public class ConfigurationValidator : BaseNopValidator<ConfigurationModel>
{
    public ConfigurationValidator(ILocalizationService localizationService)
    {
        RuleFor(x => x.GoogleMapsApiKey)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Shipping.GeofenceDelivery.Fields.GoogleMapsApiKey.Required"));
    }
}
