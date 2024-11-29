using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncLogServices;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

public class ErpAccountCreditSyncService : IErpAccountCreditSyncService
{
    #region Fields

    private readonly ISyncLogService _erpSyncLogService;
    private readonly IErpAccountService _erpAccountService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly IErpDataClearCacheService _erpDataClearCacheService;
    private readonly IErpIntegrationPluginManager _erpIntegrationPluginManager;

    #endregion

    #region Ctor

    public ErpAccountCreditSyncService(ISyncLogService erpSyncLogService,
        IErpAccountService erpAccountService,
        IErpSalesOrgService erpSalesOrgService,
        IErpDataClearCacheService erpDataClearCacheService,
        IErpIntegrationPluginManager erpIntegrationPluginService)
    {
        _erpSyncLogService = erpSyncLogService;
        _erpAccountService = erpAccountService;
        _erpSalesOrgService = erpSalesOrgService;
        _erpDataClearCacheService = erpDataClearCacheService;
        _erpIntegrationPluginManager = erpIntegrationPluginService;
    }

    #endregion

    #region Method

    public virtual async Task<bool> IsErpAccountCreditSyncSuccessfulAsync()
    {
        var erpIntegrationPlugin = await _erpIntegrationPluginManager.LoadActiveERPIntegrationPlugin();

        if (erpIntegrationPlugin is null)
        {
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpAccountCreditSyncTaskName,
                ErpSyncLevel.Account,
                "No integration method found.");

            return false;
        }

        try
        {
            #region Data collections

            var listOfSalesOrgs = new List<ErpSalesOrg>();
            var salesOrgCode = await erpIntegrationPlugin.GetSalesOrgCodeFromIntegrationSettings();
            if (!string.IsNullOrWhiteSpace(salesOrgCode))
            {
                var salesOrg = (await _erpSalesOrgService.GetAllErpSalesOrgAsync(code: salesOrgCode)).FirstOrDefault();

                if (salesOrg == null)
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpAccountCreditSyncTaskName,
                        ErpSyncLevel.Account,
                        $"No Sales org found with Sales org code: {salesOrgCode}. Unable to run {ErpDataSchedulerDefaults.ErpAccountCreditSyncTaskName}.");

                    return false;
                }
                else
                {
                    listOfSalesOrgs.Add(salesOrg);
                }
            }
            else
            {
                var salesOrgs = await _erpSalesOrgService.GetAllErpSalesOrgsAsync();

                if (salesOrgs.Any())
                {
                    listOfSalesOrgs.AddRange(salesOrgs);
                }
            }

            var erpAccountUpdateList = new List<ErpAccount>();
            var syncStartTime = DateTime.UtcNow.AddMinutes(-10);

            #endregion

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpAccountCreditSyncTaskName,
                ErpSyncLevel.Account,
                "Erp Account Credit Sync started.");

            foreach (var salesOrg in listOfSalesOrgs)
            {
                var oldErpAccounts = await _erpAccountService.GetAllErpAccountsAsync(salesOrgId: salesOrg.Id);

                if (oldErpAccounts == null || oldErpAccounts != null && oldErpAccounts.Count == 0)
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpAccountCreditSyncTaskName,
                        ErpSyncLevel.Account,
                        $"No Erp Accounts found with the Sales org : {salesOrg.Name}");

                    continue;
                }

                var isError = false;
                var start = "0";
                var lastSyncedErpAccountNumber = string.Empty;
                var totalSyncedSoFar = 0;

                while (true)
                {
                    var erpGetRequestModel = new ErpGetRequestModel
                    {
                        Start = start,
                        Location = salesOrg.Code
                    };

                    var response = await erpIntegrationPlugin.GetAllAccountCreditFromErpAsync(erpGetRequestModel);

                    if (response.ErpResponseModel.IsError)
                    {
                        isError = true;

                        await _erpSyncLogService.SyncLogSaveOnFileAsync(
                            ErpDataSchedulerDefaults.ErpAccountCreditSyncTaskName,
                            ErpSyncLevel.Account,
                            response.ErpResponseModel.ErrorShortMessage,
                            response.ErpResponseModel.ErrorFullMessage);
                        break;
                    }
                    else if (response.Data is null)
                    {
                        isError = false;
                        break;
                    }

                    start = response.ErpResponseModel.Next;

                    var responseData = response.Data
                        .Where(x => !string.IsNullOrWhiteSpace(x.AccountNumber))
                        .GroupBy(x => x.AccountNumber)
                        .Select(g => g.Last());

                    foreach (var erpAccount in responseData)
                    {
                        var oldErpAccount = oldErpAccounts.FirstOrDefault(x => x.AccountNumber == erpAccount.AccountNumber);

                        if (oldErpAccount is null)
                        {
                            continue;
                        }

                        oldErpAccount.CreditLimit = erpAccount.CreditLimit ?? 0;
                        oldErpAccount.CurrentBalance = erpAccount.CurrentBalance ?? 0;
                        oldErpAccount.CreditLimitAvailable = erpAccount.CreditLimitAvailable ?? 0;

                        oldErpAccount.UpdatedById = 1;
                        oldErpAccount.UpdatedOnUtc = DateTime.UtcNow;
                        oldErpAccount.LastErpAccountSyncDate = DateTime.UtcNow;

                        erpAccountUpdateList.Add(oldErpAccount);

                        lastSyncedErpAccountNumber = oldErpAccount.AccountNumber;
                        totalSyncedSoFar++;
                    }

                    if (erpAccountUpdateList.Count != 0)
                    {
                        await _erpAccountService.UpdateErpAccountsAsync(erpAccountUpdateList);
                        await _erpDataClearCacheService.ClearCacheOfEntities(erpAccountUpdateList);
                        erpAccountUpdateList.Clear();
                    }
                }

                if (!isError)
                {
                    //await _erpAccountService.InActiveAllOldAccount(syncStartTime);
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                            ErpDataSchedulerDefaults.ErpAccountCreditSyncTaskName,
                            ErpSyncLevel.Account,
                            $"Erp Account Credit sync is successful for Sales Org: {salesOrg.Name}. The accounts which were updated before " +
                            $"{syncStartTime} are deactivated.");
                }
                else
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                            ErpDataSchedulerDefaults.ErpAccountCreditSyncTaskName,
                            ErpSyncLevel.Account,
                            $"Erp Account Credit sync is partially or not successful for Sales Org: {salesOrg.Name}");
                }

                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    ErpDataSchedulerDefaults.ErpAccountCreditSyncTaskName,
                    ErpSyncLevel.Account,
                    (!string.IsNullOrWhiteSpace(lastSyncedErpAccountNumber) ?
                    $"The last synced Erp Account: {lastSyncedErpAccountNumber}, for Sales Org: {salesOrg.Name}. " : string.Empty) +
                    $"Total synced in this session: {totalSyncedSoFar}");
            }

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpAccountCreditSyncTaskName,
                ErpSyncLevel.Account,
                "Erp Account Credit Sync ended.");

            return true;
        }
        catch (Exception ex)
        {
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpAccountCreditSyncTaskName,
                ErpSyncLevel.Account,
                ex.Message,
                ex.StackTrace);

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpAccountCreditSyncTaskName,
                ErpSyncLevel.Account,
                "Erp Account Credit Sync ended.");

            return false;
        }
    }

    #endregion
}