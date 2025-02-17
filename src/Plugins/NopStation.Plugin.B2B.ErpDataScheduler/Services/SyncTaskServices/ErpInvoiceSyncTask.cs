using NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Models.PartialSyncModels;
using Quartz;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

[DisallowConcurrentExecution]
public partial class ErpInvoiceSyncTask : IJob
{
    #region Fields

    private readonly IErpInvoiceSyncService _erpInvoiceSyncService;

    #endregion

    #region Ctor

    public ErpInvoiceSyncTask(IErpInvoiceSyncService erpInvoiceSyncService)
    {
        _erpInvoiceSyncService = erpInvoiceSyncService;
    }

    #endregion

    #region Methods

    public async Task Execute(IJobExecutionContext context)
    {
        if (context.JobDetail.JobDataMap.TryGetBooleanValue(ErpDataSchedulerDefaults.JobShouldExecute, out var shouldExecute) && shouldExecute)
        {
            context.MergedJobDataMap.TryGetBoolean(ErpDataSchedulerDefaults.IsManualTrigger, out var isManualTrigger);
            context.MergedJobDataMap.TryGetBoolean(ErpDataSchedulerDefaults.IsIncrementalSync, out var isIncrementalSync);
            context.MergedJobDataMap.TryGetString(nameof(ErpInvoicePartialSyncModel.ErpAccountNumber), out var erpAccountNumber);

            await _erpInvoiceSyncService.IsErpInvoiceSyncSuccessfulAsync(erpAccountNumber, isManualTrigger, isIncrementalSync, context.CancellationToken);
        }
    }

    #endregion
}