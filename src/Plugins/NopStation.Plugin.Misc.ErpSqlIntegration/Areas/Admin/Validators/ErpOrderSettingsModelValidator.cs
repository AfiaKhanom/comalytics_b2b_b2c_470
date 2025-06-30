using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Misc.ErpSqlIntegration.Areas.Admin.Models;
using NopStation.Plugin.Misc.ErpSqlIntegration.Services;

namespace NopStation.Plugin.Misc.ErpSqlIntegration.Areas.Admin.Validators;

public partial class ErpOrderSettingsModelValidator : BaseNopValidator<ErpOrderSettingsModel>
{
    public ErpOrderSettingsModelValidator(ILocalizationService localizationService, ISqlIntegrationService sqlIntegrationService)
    {
        RuleFor(x => x.ErpOrderPayloadRootKey)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Admin.ErpOrderSettingsModel.Fields.ErpOrderPayloadRootKey.Required"));

        RuleFor(x => x.ErpOrderItemDataSettingsModel.ErpOrderPayloadLinesKey)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Admin.ErpOrderSettingsModel.ErpOrderItemDataSettingsModel.Fields.ErpOrderPayloadLinesKey.Required"));

        RuleFor(x => x.AdditionalHardCodedValues).Must((x, s, context) =>
        {
            var validation = sqlIntegrationService.ValidateAdditionalHardCodedValuesSettings(x.AdditionalHardCodedValues);
            if (validation.IsValid)
            {
                return true;
            }

            context.AddFailure(validation.ErrorMessage);
            return false;
        });

        RuleFor(x => x.ErpOrderItemDataSettingsModel.AdditionalHardCodedValues).Must((x, s, context) =>
        {
            var validation = sqlIntegrationService.ValidateAdditionalHardCodedValuesSettings(x.ErpOrderItemDataSettingsModel.AdditionalHardCodedValues);
            if (validation.IsValid)
            {
                return true;
            }

            context.AddFailure(validation.ErrorMessage);
            return false;
        });
    }
}