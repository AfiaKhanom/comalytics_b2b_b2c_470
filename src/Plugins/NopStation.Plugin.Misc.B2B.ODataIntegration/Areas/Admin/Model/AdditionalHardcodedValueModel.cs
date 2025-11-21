using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.Areas.Admin.Model;
public record AdditionalHardcodedValueModel : ODataIntegrationModel
{

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.AdditionalHardcodedValueModel.Fields.AdditionalHardCodedValues")]
    public string? AdditionalHardCodedValues { get; set; }
}

