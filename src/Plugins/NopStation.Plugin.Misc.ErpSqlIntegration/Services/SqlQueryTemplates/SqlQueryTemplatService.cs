using Nop.Core;
using Nop.Data;
using NopStation.Plugin.Misc.ErpSqlIntegration.Domain;

namespace NopStation.Plugin.Misc.ErpSqlIntegration.Services.SqLQueryTemplates;

public class SqlQueryTemplatService : ISqlQueryTemplatService
{
    private readonly IRepository<SqlQueryTemplate> _sqlQueryTemplateRepository;

    public SqlQueryTemplatService(IRepository<SqlQueryTemplate> sqlQueryTemplaterepository)
    {
        _sqlQueryTemplateRepository = sqlQueryTemplaterepository;
    }

    public async Task<SqlQueryTemplate> GetSqlQueryTemplateByIdAsync(int id)
    {
        return await _sqlQueryTemplateRepository.GetByIdAsync(id);
    }

    public async Task<SqlQueryTemplate> GetSqlQueryTemplateBySyncLevelIdAsync(int syncLevelId)
    {
        return await _sqlQueryTemplateRepository.Table.Where(q => q.ErpSyncLevelId == syncLevelId).FirstOrDefaultAsync();
    }

    public async Task UpdateSqlQueryTemplateAsync(SqlQueryTemplate sqlQueryTemplate)
    {
        await _sqlQueryTemplateRepository.UpdateAsync(sqlQueryTemplate);
    }

    public async Task InsertSqlQueryTemplateAsync(SqlQueryTemplate sqlQueryTemplate)
    {
        await _sqlQueryTemplateRepository.InsertAsync(sqlQueryTemplate);
    }

    public async Task<IPagedList<SqlQueryTemplate>> GetAllSqlQueryTemplatesAsync(int storeId = 0,
        int pageIndex = 0,
        int pageSize = int.MaxValue)
    {
        return await _sqlQueryTemplateRepository.GetAllPagedAsync(async query =>
        {
            query = query.OrderBy(q => q.Id);
            return query;
        }, pageIndex, pageSize);
    }
}
