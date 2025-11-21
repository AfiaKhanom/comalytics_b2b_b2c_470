using Nop.Core.Configuration;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.Settings;

public class ErpAccountSetting : ISettings
{
    public string? AccountNumber { get; set; }
    public string? AccountName { get; set; }
    public string? ErpSalesOrgCode { get; set; }
    public string? BillingSuburb { get; set; }
    public string? BillingName { get; set; }
    public string? VatNumber { get; set; }
    public string? CreditLimit { get; set; }
    public string? CreditLimitAvailable { get; set; }
    public string? CurrentBalance { get; set; }
    public string? AllowOverspend { get; set; }
    public string? PreFilterFacets { get; set; }
    public string? PaymentTypeCode { get; set; }
    public string? PriceGroupCode { get; set; }
    public string? PercentageOfStockAllowed { get; set; }
    public string? Address1 { get; set; }
    public string? Address2 { get; set; }
    public string? Address3 { get; set; }
    public string? StateProvince { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? ZipPostalCode { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? DeliveryRoute { get; set; }
    public string? CompanyNo { get; set; }
    public string? IsActive { get; set; }
    public string? IsDeleted { get; set; }
    public string? OverrideBackOrderingConfigSetting { get; set; }
    public string? AllowAccountsBackOrdering { get; set; }
    public string? AllowAccountsAddressEditOnCheckout { get; set; }
    public string? LoyaltyBalance { get; set; }
    public string? MinimumOrderValue { get; set; }
    public string? LoyaltyCardNumber { get; set; }
}
