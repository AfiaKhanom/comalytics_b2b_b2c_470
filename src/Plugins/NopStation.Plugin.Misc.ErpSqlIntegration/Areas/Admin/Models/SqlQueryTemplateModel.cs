using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.ErpSqlIntegration.Areas.Admin.Models;

public partial record SqlQueryTemplateModel : BaseNopEntityModel
{
    [NopResourceDisplayName("NopStation.Plugin.Misc.ErpSqlIntegration.Admin.SQLQueryTemplateModel.Query")]
    public string Query { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.Misc.ErpSqlIntegration.Admin.SQLQueryTemplateModel.ErpSyncLevel")]
    public int ErpSyncLevelId { get; set; }

    public IList<SelectListItem> ErpSyncLevels { get; set; }
}