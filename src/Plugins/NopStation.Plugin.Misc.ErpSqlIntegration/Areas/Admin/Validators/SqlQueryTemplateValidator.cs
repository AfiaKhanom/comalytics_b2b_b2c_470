using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Misc.ErpSqlIntegration.Areas.Admin.Models;
using NopStation.Plugin.Misc.ErpSqlIntegration.Domain;

namespace NopStation.Plugin.Misc.ErpSqlIntegration.Areas.Admin.Validators.SqlQueryTemplateValidator;

public partial class SqlQueryTemplateValidator : BaseNopValidator<SqlQueryTemplateModel>
{
    public SqlQueryTemplateValidator(ILocalizationService localizationService)
    {
        RuleFor(x => x.Query)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Admin.SqlQueryTemplate.Fields.Query.Required"));

        RuleFor(x => x.ErpSyncLevelId)
            .GreaterThan(0)
            .WithMessageAwait(localizationService.GetResourceAsync("Admin.SqlQueryTemplate.Fields.ErpSyncLevelId.ShouldMoreThanZero"));

        SetDatabaseValidationRules<SqlQueryTemplate>();
    }
}