using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.Areas.Admin.Model;
public record ErpBillingAddressModel : BaseNopModel, ISettingsModel
{
    public int ActiveStoreScopeConfiguration { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpAddressModel.Fields.Name")]
    public string? Name { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpAddressModel.Fields.Email")]
    public string? Email { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpAddressModel.Fields.Company")]
    public string? Company { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpAddressModel.Fields.Address1")]
    public string? Address1 { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpAddressModel.Fields.Address2")]
    public string? Address2 { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpAddressModel.Fields.Address3")]
    public string? Address3 { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpAddressModel.Fields.City")]
    public string? City { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpAddressModel.Fields.StateProvince")]
    public string? StateProvince { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpAddressModel.Fields.Region")]
    public string? Region { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpAddressModel.Fields.ZipPostalCode")]
    public string? ZipPostalCode { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpAddressModel.Fields.Country")]
    public string? Country { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpAddressModel.Fields.PhoneNumber")]
    public string? PhoneNumber { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpAddressModel.Fields.Suburb")]
    public string? Suburb { get; set; }
}

