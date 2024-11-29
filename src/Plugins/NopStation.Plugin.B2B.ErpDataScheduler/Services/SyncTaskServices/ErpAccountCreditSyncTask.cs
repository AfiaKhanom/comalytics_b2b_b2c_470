using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskScheduler;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

public partial class ErpAccountCreditSyncTask : ISyncTask
{
    #region Fields

    private readonly IErpAccountCreditSyncService _erpAccountCreditSyncService;

    #endregion

    #region Ctor

    public ErpAccountCreditSyncTask(IErpAccountCreditSyncService erpAccountCreditSyncService)
    {
        _erpAccountCreditSyncService = erpAccountCreditSyncService;
    }

    #endregion

    #region Methods

    public virtual async Task ExecuteAsync()
    {
        await _erpAccountCreditSyncService.IsErpAccountCreditSyncSuccessfulAsync();
    }

    #endregion
}