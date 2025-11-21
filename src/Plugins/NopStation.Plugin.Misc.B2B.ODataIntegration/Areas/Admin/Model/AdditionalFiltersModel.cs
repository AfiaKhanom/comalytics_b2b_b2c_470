using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.Areas.Admin.Model;
public record AdditionalFiltersModel : BaseNopModel, ISettingsModel
{
    public int ActiveStoreScopeConfiguration { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.AdditionalFiltersModel.Fields.AdditionalFilters")]
    public string? AdditionalFilters { get; set; }
    public bool AdditionalFilters_OverrideForStore { get; set; }
}