using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.Areas.Admin.Model;
public record ErpCreateShipToAddressSettingsModel : AdditionalHardcodedValueModel
{
    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpCreateShipToAddressSettingsModel.Fields.AccountNumber")]
    public string? AccountNumber { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpCreateShipToAddressSettingsModel.Fields.ShipToCode")]
    public string? ShipToCode { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpCreateShipToAddressSettingsModel.Fields.ShipToName")]
    public string? ShipToName { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpCreateShipToAddressSettingsModel.Fields.Country")]
    public string? Country { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpCreateShipToAddressSettingsModel.Fields.City")]
    public string? City { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpCreateShipToAddressSettingsModel.Fields.Address1")]
    public string? Address1 { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpCreateShipToAddressSettingsModel.Fields.Address2")]
    public string? Address2 { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpCreateShipToAddressSettingsModel.Fields.PostalCode")]
    public string? PostalCode { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpCreateShipToAddressSettingsModel.Fields.ShipToPhone")]
    public string? ShipToPhone { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpCreateShipToAddressSettingsModel.Fields.ShipToEmail")]
    public string? ShipToEmail { get; set; }
}