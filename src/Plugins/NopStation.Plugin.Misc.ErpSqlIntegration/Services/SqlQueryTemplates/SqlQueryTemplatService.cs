using System.Text.RegularExpressions;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.Misc.ErpSqlIntegration.Domain;

namespace NopStation.Plugin.Misc.ErpSqlIntegration.Services.SqLQueryTemplates;

public class SqlQueryTemplatService : ISqlQueryTemplatService
{
    private readonly IRepository<SqlQueryTemplate> _sqlQueryTemplateRepository;

    private static readonly Regex _forbiddenPattern = new Regex(
        @"\b(DROP|DELETE|TRUNCATE|ALTER|UPDATE|INSERT|MERGE|EXEC|CREATE|GRANT|REVOKE|DENY|BACKUP|RESTORE|SHUTDOWN|DBCC|XP_|SP_)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    );

    public SqlQueryTemplatService(IRepository<SqlQueryTemplate> sqlQueryTemplaterepository)
    {
        _sqlQueryTemplateRepository = sqlQueryTemplaterepository;
    }

    public async Task<SqlQueryTemplate> GetSqlQueryTemplateByIdAsync(int id)
    {
        if (id == 0)
            return null;
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

    public (bool IsSafe, string Message) IsSafeQuery(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return (false, "Error: The provided SQL query is empty.");

        // Normalize spaces and trim
        query = Regex.Replace(query.Trim(), @"\s+", " ");

        // Check for forbidden keywords using regex match
        var match = _forbiddenPattern.Match(query);
        if (!match.Success)
            return (true, "");

        return (false, $"Invalid query: The keyword '{match.Value}' is not allowed. Only read-only queries are permitted.");
    }
}