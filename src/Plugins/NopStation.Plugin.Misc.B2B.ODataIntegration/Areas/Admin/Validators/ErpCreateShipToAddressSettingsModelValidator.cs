using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Misc.B2B.ODataIntegration.Areas.Admin.Model;
using NopStation.Plugin.Misc.B2B.ODataIntegration.Services;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.Areas.Admin.Validators;
public partial class ErpCreateShipToAddressSettingsModelValidator : BaseNopValidator<ErpCreateShipToAddressSettingsModel>
{
    public ErpCreateShipToAddressSettingsModelValidator(ILocalizationService localizationService, ID365Service d365Service)
    {
        RuleFor(x => x.AdditionalHardCodedValues).Must((x, s, context) =>
        {
            var validation = d365Service.ValidateAdditionalHardCodedValuesSettings(x.AdditionalHardCodedValues);
            if (validation.IsValid)
                return true;

            context.AddFailure(validation.ErrorMessage);
            return false;
        });
    }
}
