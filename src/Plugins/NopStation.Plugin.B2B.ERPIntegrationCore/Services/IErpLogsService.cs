using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services;

public interface IErpLogsService
{
    Task InsertErpLogAsync(ErpLogLevel logLevel, ErpSyncLevel syncLevel, string shortMessage, string fullMessage = "", Customer customer = null, bool shouldLog = true);

    Task UpdateErpLogAsync(ErpLogs erpLog);

    Task InformationAsync(string message, ErpSyncLevel syncLevel, Exception exception = null, Customer customer = null, bool shouldLog = true);

    Task WarningAsync(string message, ErpSyncLevel syncLevel, Exception exception = null, Customer customer = null, bool shouldLog = true);

    Task ErrorAsync(string message, ErpSyncLevel syncLevel, Exception exception = null, Customer customer = null, bool shouldLog = true);

    Task<ErpLogs> GetErpLogByIdAsync(int id);

    Task<IPagedList<ErpLogs>> GetAllErpLogsAsync(string ipAddress, string message, int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false, int logLevelId = 0, int syncLevelId = 0, string nopCustomerEmail = null, DateTime? createdFrom = null, DateTime? createdTo = null);

    Task<IList<ErpLogs>> GetErpLogsByIdsAsync(int[] erpLogIds);

    Task ClearLogAsync(DateTime? olderThan = null);

    Task DeleteErpLogsAsync(IList<ErpLogs> erpLogs);

    Task DeleteErpLogByIdAsync(int id);
}
