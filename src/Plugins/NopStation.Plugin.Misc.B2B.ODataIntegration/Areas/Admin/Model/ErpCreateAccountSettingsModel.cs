using Nop.Web.Framework.Mvc.ModelBinding;
using NopStation.Plugin.Misc.B2B.ODataIntegration.Areas.Admin.Model;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.Areas.Admin.Models;

public record ErpCreateAccountSettingsModel : AdditionalHardcodedValueModel
{
    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Fields.AccountName")]
    public string? AccountName { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Fields.AccountNumber")]
    public string? AccountNumber { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Fields.Email")]
    public string? Email { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Fields.Address1")]
    public string? Address1 { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Fields.Address2")]
    public string? Address2 { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Fields.Address3")]
    public string? Address3 { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Fields.ZipPostalCode")]
    public string? ZipPostalCode { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Fields.PhoneNumber")]
    public string? PhoneNumber { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Fields.FaxNumber")]
    public string? FaxNumber { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Fields.ContactName")]
    public string? ContactName { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Fields.City")]
    public string? City { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Fields.County")]
    public string? County { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Fields.Country")]
    public string? Country { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Fields.StateProvince")]
    public string? StateProvince { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpCreateAccountSettingsModel.Fields.VatNumber")]
    public string? VatNumber { get; set; }

}
