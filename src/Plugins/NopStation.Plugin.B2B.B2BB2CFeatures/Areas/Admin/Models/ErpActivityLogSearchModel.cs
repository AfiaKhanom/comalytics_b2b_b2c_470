using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;

public record ErpActivityLogSearchModel : BaseSearchModel
{
    #region Ctor

    public ErpActivityLogSearchModel()
    {
        AvailableErpSyncLabel = new List<SelectListItem>();
        AvailableActivityType = new List<SelectListItem>();
    }

    #endregion

    #region Properties

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpLog.Field.ErpLogLevelId")]
    public int ActivityLogLevelId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpLog.Field.ErpSyncLabelId")]
    public int ErpSyncLabelId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpLog.Field.CreatedFrom")]
    [UIHint("DateNullable")]
    public DateTime? CreatedFrom { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpLog.Field.CreatedTo")]
    [UIHint("DateNullable")]
    public DateTime? CreatedTo { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpLog.Field.IpAddress")]
    public string IpAddress { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpLog.Field.NopCustomerEmail")]
    public string NopCustomerEmail { get; set; }

    public IList<SelectListItem> AvailableActivityType { get; set; }
    public IList<SelectListItem> AvailableErpSyncLabel { get; set; }

    #endregion
}
