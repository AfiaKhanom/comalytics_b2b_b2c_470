using NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Models.PartialSyncModels;
using Quartz;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

[DisallowConcurrentExecution]
public partial class ErpOrderSyncTask : IJob
{
    #region Fields

    private readonly IErpOrderSyncService _erpOrderSyncService;

    #endregion

    #region Ctor

    public ErpOrderSyncTask(IErpOrderSyncService erpOrderSyncService)
    {
        _erpOrderSyncService = erpOrderSyncService;
    }

    #endregion

    #region Methods

    public async Task Execute(IJobExecutionContext context)
    {
        if (context.JobDetail.JobDataMap.TryGetBooleanValue(ErpDataSchedulerDefaults.JobShouldExecute, out var shouldExecute) && shouldExecute)
        {
            context.MergedJobDataMap.TryGetBoolean(ErpDataSchedulerDefaults.IsManualTrigger, out var isManualTrigger);
            context.MergedJobDataMap.TryGetBoolean(ErpDataSchedulerDefaults.IsIncrementalSync, out var isIncrementalSync);
            context.MergedJobDataMap.TryGetString(nameof(ErpOrderPartialSyncModel.ErpAccountNumber), out var erpAccountNumber);
            context.MergedJobDataMap.TryGetString(nameof(ErpOrderPartialSyncModel.OrderNumber), out var orderNumber);

            await _erpOrderSyncService.IsErpOrderSyncSuccessfulAsync(erpAccountNumber, orderNumber, isManualTrigger, isIncrementalSync, context.CancellationToken);
        }
    }

    #endregion
}