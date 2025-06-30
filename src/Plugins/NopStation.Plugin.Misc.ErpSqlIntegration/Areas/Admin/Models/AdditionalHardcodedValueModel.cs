using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.ErpSqlIntegration.Areas.Admin.Models;
public record AdditionalHardcodedValueModel : BaseNopModel, ISettingsModel
{
    public int ActiveStoreScopeConfiguration { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.AdditionalHardcodedValueModel.Fields.AdditionalHardCodedValues")]
    public string? AdditionalHardCodedValues { get; set; }
}

