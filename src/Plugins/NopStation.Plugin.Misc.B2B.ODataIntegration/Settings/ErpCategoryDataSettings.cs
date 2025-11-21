using Nop.Core.Configuration;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.Settings;
public class ErpCategoryDataSettings : ISettings
{
    public string? CategoryName { get; set; }
    public string? Description { get; set; }
}
