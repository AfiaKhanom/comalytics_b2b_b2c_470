namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

public interface IErpAccountCreditSyncService
{
    Task<bool> IsErpAccountCreditSyncSuccessfulAsync();
}
