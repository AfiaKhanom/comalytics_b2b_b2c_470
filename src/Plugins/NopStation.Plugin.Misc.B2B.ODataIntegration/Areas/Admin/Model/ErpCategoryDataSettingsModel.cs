using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.Areas.Admin.Model;
public record ErpCategoryDataSettingsModel : BaseNopModel, ISettingsModel
{
    public int ActiveStoreScopeConfiguration { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpCategoryDataSettingsModel.Fields.CategoryName")]
    public string? CategoryName { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpCategoryDataSettingsModel.Fields.Description")]
    public string? Description { get; set; }
}

