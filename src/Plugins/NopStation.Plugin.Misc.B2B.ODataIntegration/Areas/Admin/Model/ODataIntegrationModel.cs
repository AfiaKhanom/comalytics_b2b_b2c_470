using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.Areas.Admin.Model;
public record ODataIntegrationModel : BaseNopModel, ISettingsModel
{
    public ODataIntegrationModel()
    {
        AvailableProperties = new List<SelectListItem>();
    }

    public int ActiveStoreScopeConfiguration { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ExcludePropertySelectedValues")]
    public IList<string>? ExcludePropertySelectedValues { get; set; }
    public IList<SelectListItem> AvailableProperties { get; set; }
}
