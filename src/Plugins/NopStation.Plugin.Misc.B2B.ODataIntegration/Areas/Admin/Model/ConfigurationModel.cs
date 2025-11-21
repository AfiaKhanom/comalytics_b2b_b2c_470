using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.Areas.Admin.Models
{
    public record ConfigurationModel : BaseNopModel, ISettingsModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.NopStation.D365Integration.Configuration.Fields.DefaultCustomerId")]
        public int DefaultCustomerId { get; set; }
        public bool DefaultCustomerId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.NopStation.D365Integration.Configuration.Fields.ErpCallTimeOut")]
        public int ErpCallTimeOut { get; set; }
        public bool ErpCallTimeOut_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.NopStation.D365Integration.Configuration.Fields.HttpCallMaxRetries")]
        public int HttpCallMaxRetries { get; set; }
        public bool HttpCallMaxRetries_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.NopStation.D365Integration.Configuration.Fields.HttpCallRestTimeInMinutes")]
        public int HttpCallRestTimeInMinutes { get; set; }
        public bool HttpCallRestTimeInMinutes_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.NopStation.D365Integration.Configuration.Fields.DefaultDateTimeFormat")]
        public string? DefaultDateTimeFormat { get; set; }
        public bool DefaultDateTimeFormat_OverrideForStore { get; set; }
        
        [NopResourceDisplayName("Plugins.NopStation.D365Integration.Configuration.Fields.AdditionalMappings")]
        public string? AdditionalMappings { get; set; }
        public bool AdditionalMappings_OverrideForStore { get; set; }

        [NopResourceDisplayName("Integration.IntegrationSecretKey")]
        public string IntegrationSecretKey { get; set; }

        public bool IntegrationSecretKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.NopStation.D365Integration.Configuration.Fields.ShippingCostItemPayload")]
        public string? ShippingCostItemPayload { get; set; }

        public bool ShippingCostItemPayload_OverrideForStore { get; set; }

        public IList<SelectListItem> AvailableCustomers { get; set; } = new List<SelectListItem>();

    }
}