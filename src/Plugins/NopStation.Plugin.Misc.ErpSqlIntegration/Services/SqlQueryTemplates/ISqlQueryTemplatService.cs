using Nop.Core;
using NopStation.Plugin.Misc.ErpSqlIntegration.Domain;

namespace NopStation.Plugin.Misc.ErpSqlIntegration.Services.SqLQueryTemplates;
public interface ISqlQueryTemplatService
{
    Task<SqlQueryTemplate> GetSqlQueryTemplateByIdAsync(int id);

    Task<SqlQueryTemplate> GetSqlQueryTemplateBySyncLevelIdAsync(int syncLevelId);

    Task UpdateSqlQueryTemplateAsync(SqlQueryTemplate sqlQueryTemplate);

    Task InsertSqlQueryTemplateAsync(SqlQueryTemplate sqlQueryTemplate);

    Task<IPagedList<SqlQueryTemplate>> GetAllSqlQueryTemplatesAsync(int storeId = 0,
        int pageIndex = 0,
        int pageSize = int.MaxValue);

    (bool IsSafe, string Message) IsSafeQuery(string query);
}
