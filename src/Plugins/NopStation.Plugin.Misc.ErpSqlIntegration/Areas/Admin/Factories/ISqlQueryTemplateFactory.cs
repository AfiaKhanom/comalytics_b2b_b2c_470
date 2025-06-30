using NopStation.Plugin.Misc.ErpSqlIntegration.Areas.Admin.Models;
using NopStation.Plugin.Misc.ErpSqlIntegration.Domain;

namespace NopStation.Plugin.Misc.ErpSqlIntegration.Areas.Admin.Factories;

public interface ISqlQueryTemplateModelFactory
{
    Task<SqlQueryTemplateSearchModel> PrepareSqlQueryTemplateModelSearchModelAsync(SqlQueryTemplateSearchModel searchModel);
    Task<SqlQueryTemplateListModel> PrepareSqlQueryTemplateListModelAsync(SqlQueryTemplateSearchModel searchModel);
    Task<SqlQueryTemplateModel> PrepareSqlQueryTemplateModelAsync(SqlQueryTemplateModel model, SqlQueryTemplate sqlQueryTemplate);
}
