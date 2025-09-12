using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.ErpSqlIntegration.Areas.Admin.Models
{
    public record ErpShippingAddressPayloadSettingsModel : AdditionalHardcodedValueModel
    {
        [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.ErpShippingAddressPayloadSettings.Fields.Name")]
        public string? Name { get; set; }

        [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.ErpShippingAddressPayloadSettings.Fields.Email")]
        public string? Email { get; set; }

        [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.ErpShippingAddressPayloadSettings.Fields.Company")]
        public string? Company { get; set; }

        [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.ErpShippingAddressPayloadSettings.Fields.Address1")]
        public string? Address1 { get; set; }

        [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.ErpShippingAddressPayloadSettings.Fields.Address2")]
        public string? Address2 { get; set; }

        [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.ErpShippingAddressPayloadSettings.Fields.Address3")]
        public string? Address3 { get; set; }

        [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.ErpShippingAddressPayloadSettings.Fields.City")]
        public string? City { get; set; }

        [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.ErpShippingAddressPayloadSettings.Fields.StateProvince")]
        public string? StateProvince { get; set; }

        [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.ErpShippingAddressPayloadSettings.Fields.Region")]
        public string? Region { get; set; }

        [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.ErpShippingAddressPayloadSettings.Fields.ZipPostalCode")]
        public string? ZipPostalCode { get; set; }

        [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.ErpShippingAddressPayloadSettings.Fields.Country")]
        public string? Country { get; set; }

        [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.ErpShippingAddressPayloadSettings.Fields.PhoneNumber")]
        public string? PhoneNumber { get; set; }

        [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.ErpShippingAddressPayloadSettings.Fields.Suburb")]
        public string? Suburb { get; set; }

        [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.ErpShippingAddressPayloadSettings.Fields.ShippingAddressPayloadKey")]
        public string? ShippingAddressPayloadKey { get; set; }
    }
}
