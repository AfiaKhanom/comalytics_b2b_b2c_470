using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using NopStation.Plugin.Misc.B2B.ODataIntegration.Areas.Admin.Model;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.Areas.Admin.Models;

public record ErpAccountSettingModel : ODataIntegrationModel
{
    public ErpAccountSettingModel()
    {
        ErpGetRequestSettingsModel = new ErpGetRequestSettingsModel();
    }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.AccountNumber")]
    public string? AccountNumber { get; set; }
    public bool  AccountNumber_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.AccountName")]
    public string? AccountName { get; set; }
    public bool  AccountName_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.Address1")]
    public string? Address1 { get; set; }
    public bool  Address1_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.Address2")]
    public string? Address2 { get; set; }
    public bool  Address2_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.Address3")]
    public string? Address3 { get; set; }
    public bool  Address3_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.City")]
    public string? City { get; set; }
    public bool  City_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.StateProvince")]
    public string? StateProvince { get; set; }
    public bool  StateProvince_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.ZipPostalCode")]
    public string? ZipPostalCode { get; set; }
    public bool  ZipPostalCode_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.Country")]
    public string? Country { get; set; }
    public bool  Country_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.PhoneNumber")]
    public string? PhoneNumber { get; set; }
    public bool  PhoneNumber_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.Email")]
    public string? Email { get; set; }
    public bool  Email_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.CompanyNo")]
    public string? CompanyNo { get; set; }
    public bool  CompanyNo_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.VatNumber")]
    public string? VatNumber { get; set; }
    public bool  VatNumber_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.CreditLimit")]
    public string? CreditLimit { get; set; }
    public bool  CreditLimit_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.ErpSalesOrgCode")]
    public string? ErpSalesOrgCode { get; set; }
    public bool  ErpSalesOrgCode_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.BillingSuburb")]
    public string? BillingSuburb { get; set; }
    public bool  BillingSuburb_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.BillingName")]
    public string? BillingName { get; set; }
    public bool  BillingName_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.CreditLimitAvailable")]
    public string? CreditLimitAvailable { get; set; }
    public bool  CreditLimitAvailable_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.CurrentBalance")]
    public string? CurrentBalance { get; set; }
    public bool  CurrentBalance_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.AllowOverspend")]
    public string? AllowOverspend { get; set; }
    public bool  AllowOverspend_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.PreFilterFacets")]
    public string? PreFilterFacets { get; set; }
    public bool  PreFilterFacets_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.PaymentTypeCode")]
    public string? PaymentTypeCode { get; set; }
    public bool  PaymentTypeCode_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.PriceGroupCode")]
    public string? PriceGroupCode { get; set; }
    public bool  PriceGroupCode_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.PercentageOfStockAllowed")]
    public string? PercentageOfStockAllowed { get; set; }
    public bool  PercentageOfStockAllowed_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.DeliveryRoute")]
    public string? DeliveryRoute { get; set; }
    public bool  DeliveryRoute_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.IsActive")]
    public string? IsActive { get; set; }
    public bool  IsActive_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.IsDeleted")]
    public string? IsDeleted { get; set; }
    public bool  IsDeleted_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.OverrideBackOrderingConfigSetting")]
    public string? OverrideBackOrderingConfigSetting { get; set; }
    public bool  OverrideBackOrderingConfigSetting_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.AllowAccountsBackOrdering")]
    public string? AllowAccountsBackOrdering { get; set; }
    public bool  AllowAccountsBackOrdering_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.AllowAccountsAddressEditOnCheckout")]
    public string? AllowAccountsAddressEditOnCheckout { get; set; }
    public bool  AllowAccountsAddressEditOnCheckout_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.LoyaltyBalance")]
    public string? LoyaltyBalance { get; set; }
    public bool  LoyaltyBalance_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.MinimumOrderValue")]
    public string? MinimumOrderValue { get; set; }
    public bool  MinimumOrderValue_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpAccountSettingModel.Fields.LoyaltyCardNumber")]
    public string? LoyaltyCardNumber { get; set; }
    public bool  LoyaltyCardNumber_OverrideForStore { get; set; }

    public ErpGetRequestSettingsModel ErpGetRequestSettingsModel { get; set; }
}
