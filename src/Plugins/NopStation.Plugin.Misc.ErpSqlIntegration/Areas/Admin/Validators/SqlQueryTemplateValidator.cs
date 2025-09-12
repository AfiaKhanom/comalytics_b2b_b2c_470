using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Misc.ErpSqlIntegration.Areas.Admin.Models;
using NopStation.Plugin.Misc.ErpSqlIntegration.Domain;
using NopStation.Plugin.Misc.ErpSqlIntegration.Services.SqLQueryTemplates;

namespace NopStation.Plugin.Misc.ErpSqlIntegration.Areas.Admin.Validators.SqlQueryTemplateValidator;

public partial class SqlQueryTemplateValidator : BaseNopValidator<SqlQueryTemplateModel>
{
    public SqlQueryTemplateValidator(ILocalizationService localizationService,
        ISqlQueryTemplatService sqlQueryTemplatService)
    {
        RuleFor(x => x.Query)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Admin.SqlQueryTemplate.Fields.Query.Required"));

        RuleFor(x => x.ErpSyncLevelId)
            .GreaterThan(0)
            .WithMessageAwait(localizationService.GetResourceAsync("Admin.SqlQueryTemplate.Fields.ErpSyncLevelId.ShouldMoreThanZero"));

        RuleFor(x => x.Query)
            .Must((model, query, context) =>
            {
                var (isSafe, errorMessage) = sqlQueryTemplatService.IsSafeQuery(query);
                if (!isSafe)
                {
                    context.MessageFormatter.AppendArgument("CustomError", errorMessage);
                    return false;
                }
                return true;
            })
            .WithMessage("{CustomError}");

        SetDatabaseValidationRules<SqlQueryTemplate>();
    }
}