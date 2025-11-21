using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.Areas.Admin.Model;

public record ErpShipToAddressSettingModel : ODataIntegrationModel
{
    public ErpShipToAddressSettingModel()
    {
        ErpGetRequestSettingsModel = new ErpGetRequestSettingsModel();

    }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.AccountNumber")]
    public string? AccountNumber { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.ShipToCode")]
    public string? ShipToCode { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.ShipToName")]
    public string? ShipToName { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.Company")]
    public string? Company { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.Address1")]
    public string? Address1 { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.Address2")]
    public string? Address2 { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.City")]
    public string? City { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.ProvinceCode")]
    public string? StateProvince { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.Suburb")]
    public string? Suburb { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.County")]
    public string? County { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.Country")]
    public string? Country { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.ZipPostalCode")]
    public string? ZipPostalCode { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.PhoneNumber")]
    public string? PhoneNumber { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.FaxNumber")]
    public string? FaxNumber { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.DeliveryNotes")]
    public string? DeliveryNotes { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.EmailAddress")]
    public string? EmailAddress { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.RepNumber")]
    public string? RepNumber { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.RepFullName")]
    public string? RepFullName { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.RepPhoneNumber")]
    public string? RepPhoneNumber { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.RepEmail")]
    public string? RepEmail { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpShipToAddressSettings.Fields.SalesOrgCode")]
    public string? SalesOrgCode { get; set; }

    public ErpGetRequestSettingsModel ErpGetRequestSettingsModel { get; set; }
}

