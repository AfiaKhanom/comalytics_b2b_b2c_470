using Nop.Core.Configuration;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration
{
    public class D365IntegrationSettings : ISettings
    {
        /// <summary>
        /// Gets or sets the HTTP call timeout in seconds
        /// </summary>
        public int ErpCallTimeOut { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of retries for HTTP calls
        /// </summary>
        public int HttpCallMaxRetries { get; set; }

        /// <summary>
        /// Gets or sets the rest time between retries in minutes
        /// </summary>
        public int HttpCallRestTimeInMinutes { get; set; }
        public string? DefaultDateTimeFormat { get; set; }

        public string AdditionalMappings { get; set; }

        public string IntegrationSecretKey { get; set; }
        public string ShippingCostItemPayload { get; set; }
    }
}