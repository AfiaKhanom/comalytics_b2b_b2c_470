using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Models;

public record ConfigurationModel : BaseNopModel, ISettingsModel
{
    public int ActiveStoreScopeConfiguration { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ErpDataScheduler.Admin.Configure.Fields.SyncFromDate")]
    [UIHint("DateTimeNullable")]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
    public DateTime? SyncFromDate { get; set; }
    public bool SyncFromDate_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ErpDataScheduler.Admin.Configure.Fields.NeedQuoteOrderCall")]
    public bool NeedQuoteOrderCall { get; set; }
    public bool NeedQuoteOrderCall_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ErpDataScheduler.Admin.Configure.Fields.EnalbeSendingEmailNotificationOnSyncError")]
    public bool EnalbeSendingEmailNotificationOnSyncError { get; set; }
    public bool EnalbeSendingEmailNotificationOnSyncError_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ErpDataScheduler.Admin.Configure.Fields.EmailAddresses")]
    public string? EmailAddresses { get; set; }
    public bool EmailAddresses_OverrideForStore { get; set; }
}
