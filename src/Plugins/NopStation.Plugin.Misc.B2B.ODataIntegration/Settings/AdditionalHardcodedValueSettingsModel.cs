using Nop.Core.Configuration;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration;

public class AdditionalHardcodedValueSettingsModel : ISettings
{
    public string? AdditionalHardCodedValues { get; set; }
}
